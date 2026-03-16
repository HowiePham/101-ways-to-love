using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class CheckTotalValue : VisualAction
{
    [SerializeField] private TotalValue totalValue;
    [SerializeField] private float targetValue;
    [SerializeField] private bool equal = true;
    [SerializeField] private bool larger;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.totalValue == null)
        {
            return;
        }

        if (this.equal)
        {
            await UniTask.WaitUntil(() => Mathf.Approximately(this.totalValue.Total, this.targetValue), cancellationToken: cancellationToken);
        }
        else if (this.larger)
        {
            await UniTask.WaitUntil(() => this.totalValue.Total > this.targetValue, cancellationToken: cancellationToken);
        }
        else
        {
            await UniTask.WaitUntil(() => this.totalValue.Total < this.targetValue, cancellationToken: cancellationToken);
        }
    }
}