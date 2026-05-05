using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.Prototypes.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualFlow;

namespace Games
{
    public class HintPlayer : MonoBehaviour
    {
        [SerializeField] private BaseHint[] hints;
        [SerializeField] private ScaleObjectHighlight[] objectHighlights;
        [SerializeField] private int totalStep;
        [SerializeField] private float idleTimeBeforeHighlight = 15f;
        private bool levelTutorial;
        private bool isAnimationPlaying;
        private bool isFingerDown;

        public bool HasHint => this.hints.Length > 0;
        public bool LevelTutorial => this.levelTutorial;
        public int TotalStep => this.totalStep;

        private readonly CancellationTokenSource tokenSource = new();
        private CancellationTokenSource highlightTimerCts;

        private void OnEnable()
        {
            Messenger.AddListener(EventKey.AnimationStart, OnAnimationStart);
            Messenger.AddListener(EventKey.AnimationComplete, OnAnimationComplete);
            Messenger.AddListener(EventKey.ActionFailed, OnActionFailed);
            Messenger.AddListener(EventKey.LevelWin, CancelIdleTimer);
            Messenger.AddListener(EventKey.StartLevelGame, RestartIdleTimer);
            LeanTouch.OnFingerDown += OnFingerDown;
            LeanTouch.OnFingerUp += OnFingerUp;
        }

        private void OnDisable()
        {
            Messenger.RemoveListener(EventKey.AnimationStart, OnAnimationStart);
            Messenger.RemoveListener(EventKey.AnimationComplete, OnAnimationComplete);
            Messenger.RemoveListener(EventKey.ActionFailed, OnActionFailed);
            Messenger.RemoveListener(EventKey.LevelWin, CancelIdleTimer);
            Messenger.RemoveListener(EventKey.StartLevelGame, RestartIdleTimer);
            LeanTouch.OnFingerDown -= OnFingerDown;
            LeanTouch.OnFingerUp -= OnFingerUp;
        }

        private void OnAnimationStart()
        {
            this.isAnimationPlaying = true;
        }

        private void OnAnimationComplete()
        {
            this.isAnimationPlaying = false;
        }

        private void OnFingerDown(LeanFinger finger)
        {
            this.isFingerDown = true;
        }

        private void OnFingerUp(LeanFinger finger)
        {
            this.isFingerDown = false;
        }

        private void OnActionFailed()
        {
            CancelIdleTimer();
            ActiveObjectHighlight(true);
        }

        private void RestartIdleTimer()
        {
            CancelIdleTimer();
            this.highlightTimerCts = new CancellationTokenSource();
            RunIdleTimer(this.highlightTimerCts.Token).Forget();
        }

        private void CancelIdleTimer()
        {
            this.highlightTimerCts?.Cancel();
            this.highlightTimerCts?.Dispose();
            this.highlightTimerCts = null;
        }

        private async UniTask RunIdleTimer(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(this.idleTimeBeforeHighlight), cancellationToken: token);
            ActiveObjectHighlight(true);
        }

        public void SetLevelTutorial(bool levelTutorial)
        {
            this.levelTutorial = levelTutorial;
        }

        public async UniTask Init()
        {
            foreach (BaseHint hint in this.hints)
            {
                await hint.Initialize();
            }
        }

        public async UniTask ShowNextHint()
        {
            if (!HasHint) return;

            var hintRunning = false;
            foreach (BaseHint hint in this.hints)
            {
                if (!hint.IsExecuting)
                {
                    continue;
                }

                hintRunning = true;
                break;
            }

            if (hintRunning)
            {
                await UniTask.CompletedTask;
                return;
            }

            for (int i = 0; i < this.hints.Length; i++)
            {
                if (!this.hints[i].IsInitialized)
                {
                    await UniTask.WaitUntil(() => this.hints[i].IsInitialized);
                }

                if (this.hints[i].Completed)
                {
                    continue;
                }

                if (this.isAnimationPlaying || this.isFingerDown)
                {
                    await UniTask.WaitUntil(() => !this.isAnimationPlaying && !this.isFingerDown,
                        cancellationToken: this.tokenSource.Token);
                }

                await this.hints[i].Execute(this.tokenSource.Token);
            }
        }

        private void ActiveObjectHighlight(bool active)
        {
            if (this.objectHighlights == null || this.objectHighlights.Length == 0)
            {
                return;
            }

            foreach (ScaleObjectHighlight objectHighlight in this.objectHighlights)
            {
                objectHighlight.EnableHighlight = active;
            }
        }

        public void CancelHint()
        {
            this.tokenSource.Cancel();
        }

        [Button]
        private void TestNextHint()
        {
            ShowNextHint();
        }

        [Button]
        private void TestHighlightObjectOn()
        {
            ActiveObjectHighlight(true);
        }

        [Button]
        private void TestHighlightObjectOff()
        {
            ActiveObjectHighlight(false);
        }

        [Button]
        public void GetHints()
        {
            this.hints = GetComponentsInChildren<BaseHint>();
            this.totalStep = this.hints.Length;
        }

        [Button]
        public void GetAllObjectHighlights()
        {
            this.objectHighlights = GetComponentsInChildren<ScaleObjectHighlight>();
        }
    }
}