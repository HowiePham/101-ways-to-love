using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class CheckTimingCount : VisualAction
{
    [SerializeField] private int completeNumber = 1;
    [SerializeField] private VisualAction[] redoActions;
    private int currentCount = 0;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.currentCount++;

        if (this.currentCount < this.completeNumber)
        {
            foreach (VisualAction action in this.redoActions)
            {
                action.Execute(cancellationToken);
            }

            await UniTask.WaitUntil(IsCompleted);
        }
    }

    private bool IsCompleted()
    {
        return this.currentCount >= this.completeNumber;
    }
}