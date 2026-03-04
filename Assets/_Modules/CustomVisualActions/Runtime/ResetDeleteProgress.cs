using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;
using VisualFlow;

public class ResetDeleteProgress : VisualAction
{
    [SerializeField] private SmoothDelete smoothDelete;
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.smoothDelete.ResetDeleteProgress();
        this.smoothDelete.ResetDeleteMaterial();
    }
}