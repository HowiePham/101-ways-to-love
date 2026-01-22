using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hint;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Games
{
    public class HintPlayerOldMechanic : MonoBehaviour
    {
        [SerializeField] private ShowHintPathOldMechanic[] hints;

        private List<ShowHintPathOldMechanic> availableHints;

        public bool HasHint => this.availableHints.Count > 0;

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        private void Awake()
        {
            this.availableHints = new List<ShowHintPathOldMechanic>(this.hints);
        }

        public async UniTask ShowNextHint()
        {
            if (!HasHint) return;
            for (int i = this.availableHints.Count - 1; i >= 0; i--)
            {
                if (!this.availableHints[i].HintedActionComplete) continue;
                this.availableHints.RemoveAt(i);
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
            this.hints = GetComponentsInChildren<ShowHintPathOldMechanic>();
        }
    }
}