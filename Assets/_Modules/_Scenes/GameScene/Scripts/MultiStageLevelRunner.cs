using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

namespace Mimi
{
    public class MultiStageLevelRunner : MonoBehaviour
    {
        [SerializeField] private VisualAction[] startingStageActions;

        public int CurrentStageIndex { private set; get; }
        public int NumberOfStages => this.startingStageActions.Length;
        private readonly ActionRunner actionRunner = new ActionRunner();

        public async UniTask Initialize()
        {
            foreach (VisualAction action in this.startingStageActions)
            {
                await action.Initialize();
            }
        }

        public async UniTask StartFromBeginning()
        {
            Cancel();
            await this.actionRunner.Run(this.startingStageActions[0]);
        }

        public async UniTask RestartCurrentStage()
        {
            Cancel();
            await this.actionRunner.Run(this.startingStageActions[CurrentStageIndex]);
        }

        public async UniTask StartNextStage()
        {
            Cancel();
            CurrentStageIndex++;
            VisualAction nextStageAction = this.startingStageActions[CurrentStageIndex];
            await this.actionRunner.Run(nextStageAction);
        }

        public void Cancel()
        {
            if (this.actionRunner.IsRunning)
            {
                this.actionRunner.Cancel();
            }
        }
    }
}