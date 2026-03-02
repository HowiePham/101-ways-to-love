using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class ResetBalanceTimingBar : VisualAction
{
    [SerializeField] private TapBalanceTimingBar balanceTimingAction;
    
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.balanceTimingAction.ResetAction();
    }
}