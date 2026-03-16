using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class SetTotalValue : VisualAction
{
    [SerializeField] private TotalValue totalValue;
    [SerializeField] private float value;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.totalValue.SetTotalValue(this.value);
    }
}