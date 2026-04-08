using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualFlow;

namespace Games
{
    public class HintPlayer : MonoBehaviour
    {
        [SerializeField] private BaseHint[] hints;
        [SerializeField] private int totalStep;
        private bool levelTutorial;
        private bool isAnimationPlaying;

        public bool HasHint => this.hints.Length > 0;
        public bool LevelTutorial => this.levelTutorial;

        public int TotalStep => this.totalStep;

        private readonly CancellationTokenSource tokenSource = new();

        private void OnEnable()
        {
            Messenger.AddListener(EventKey.AnimationStart, OnAnimationStart);
            Messenger.AddListener(EventKey.AnimationComplete, OnAnimationComplete);
        }

        private void OnDisable()
        {
            Messenger.RemoveListener(EventKey.AnimationStart, OnAnimationStart);
            Messenger.RemoveListener(EventKey.AnimationComplete, OnAnimationComplete);
        }

        private void OnAnimationStart()
        {
            this.isAnimationPlaying = true;
        }

        private void OnAnimationComplete()
        {
            this.isAnimationPlaying = false;
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

                if (this.isAnimationPlaying)
                {
                    await UniTask.WaitUntil(() => !this.isAnimationPlaying,
                        cancellationToken: this.tokenSource.Token);
                }

                await this.hints[i].Execute(this.tokenSource.Token);
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
        public void GetHints()
        {
            this.hints = GetComponentsInChildren<BaseHint>();
            this.totalStep = this.hints.Length;
        }
    }
}