using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameScenes
{
    public class Timeline : MonoBehaviour
    {
        [SerializeField, InlineEditor] private VisualAction startingAction;

        private CancellationTokenSource tokenSource;

        public async UniTask Play()
        {
            this.tokenSource = new CancellationTokenSource();
            await this.startingAction.Initialize();
            await this.startingAction.Execute(this.tokenSource.Token);
        }

        public void Cancel()
        {
            this.tokenSource.Cancel();
        }
    }
}