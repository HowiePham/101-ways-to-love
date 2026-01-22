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

        private List<BaseHint> availableHints;

        public bool HasHint => this.availableHints.Count > 0;

        private readonly CancellationTokenSource tokenSource = new();

        private void Awake()
        {
            this.availableHints = new List<BaseHint>(this.hints);
        }

        public async UniTask ShowNextHint()
        {
            if (!HasHint) return;
            for (int i = 0; i < this.availableHints.Count; i++)
            {
                if (this.availableHints[i].Completed)
                    this.availableHints.Remove(this.availableHints[i]);
            }

            await this.availableHints[0].Execute(this.tokenSource.Token);
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
        private void GetHints()
        {
            this.hints = GetComponentsInChildren<BaseHint>();
        }
    }
}