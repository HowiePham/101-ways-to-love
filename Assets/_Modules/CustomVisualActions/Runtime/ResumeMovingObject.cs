using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class ResumeMovingObject : VisualAction
{
    [SerializeField] private MoveGameObjectFollowWay movingObject;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.movingObject == null)
        {
            return;
        }

        this.movingObject.ResumeMoving();
    }
}