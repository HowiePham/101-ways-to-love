using Cysharp.Threading.Tasks;
using Mimi.Actions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions
{
    public class VisualActionMonoRunner : MonoBehaviour, IActionRunner
    {
        [SerializeField, InlineEditor] private VisualAction startingAction;
        [SerializeField] private bool autoStart = true;

        private ActionRunner actionRunner;
        public bool IsRunning => this.actionRunner.IsRunning;
        public bool IsPaused => this.actionRunner.IsPaused;

        private void Awake()
        {
            this.actionRunner = new ActionRunner();
        }

        private async void Start()
        {
            if (this.autoStart)
            {
                await Run();
            }
        }

        private void OnDestroy()
        {
            Cancel();
            this.actionRunner.Dispose();
            this.actionRunner = null;
        }

        [Button]
        public async UniTask Run()
        {
            await this.actionRunner.Run(this.startingAction);
        }

        public async UniTask Run(IAsyncAction action)
        {
            await UniTask.CompletedTask;
        }

        [Button]
        public void Cancel()
        {
            this.actionRunner.Cancel();
        }

        public void Pause()
        {
            this.actionRunner.Pause();
        }

        public void Resume()
        {
            this.actionRunner.Resume();
        }

        public void Dispose()
        {
            this.actionRunner.Dispose();
        }
    }
}