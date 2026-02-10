using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        public bool HasHint => this.hints.Length > 0;
        public bool LevelTutorial => this.levelTutorial;

        public int TotalStep => this.totalStep;

        private readonly CancellationTokenSource tokenSource = new();

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