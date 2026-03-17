using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class ChangeTotalValue : VisualAction
{
    [SerializeField] private float changedValue;
    [SerializeField] private TotalValue totalValue;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        float newVal = this.changedValue + this.totalValue.Total;

        this.totalValue.SetTotalValue(newVal);
    }
}