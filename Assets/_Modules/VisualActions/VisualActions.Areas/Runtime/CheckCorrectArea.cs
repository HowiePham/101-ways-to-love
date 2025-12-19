using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

namespace VisualActions.Areas
{
    public class CheckCorrectArea : VisualAction
    {
        [SerializeField] private BaseArea correctArea;
        [SerializeField] private VisualAction completedAction;
        [SerializeField] private VisualAction executedAction;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (!this.correctArea.Active)
            {
                //Correct
                if (this.completedAction != null)
                {
                    await this.completedAction.Execute(cancellationToken);
                }
                else
                {
                    await UniTask.CompletedTask;
                }
            }
            else
            {
                //InCorrect
                if (this.executedAction != null)
                {
                    await this.executedAction.Execute(cancellationToken);
                }
                else
                {
                    await UniTask.CompletedTask;
                }
            }
        }
    }
}