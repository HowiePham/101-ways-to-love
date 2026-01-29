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
        private bool levelTutorial;

        public bool HasHint => this.hints.Length > 0;
        public int HintStepNumber => this.hints.Length;

        private readonly CancellationTokenSource tokenSource = new();

        private void Start()
        {
            Init();
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
            Debug.Log($"--- (Hint) Has hint: {HasHint}");

            if (!HasHint) return;

            Debug.Log($"--- (Hint) Showing next hint!");
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
                Debug.Log($"--- (Hint) Hint is running");

                await UniTask.CompletedTask;
                return;
            }

            for (int i = 0; i < this.hints.Length; i++)
            {
                if (!this.hints[i].IsInitialized)
                {
                    Debug.Log($"--- (Hint) await hint initialized");
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
        }
    }
}