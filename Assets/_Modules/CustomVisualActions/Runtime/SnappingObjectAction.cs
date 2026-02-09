using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class SnappingObjectAction : VisualAction
{
    [SerializeField] private BoxSnapping boxSnapping;
    [SerializeField] private bool waitSnapping = true;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.waitSnapping)
        {
            await this.boxSnapping.SnapNextObject();
        }
        else
        {
            this.boxSnapping.SnapNextObject();
        }
    }
}