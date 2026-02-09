using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;

public class JumpByTween : VisualAction
{
    [SerializeField] private Transform moveObject;
    [SerializeField] private Transform targetPos;
    [SerializeField] private float duration;
    [SerializeField] private float jumpForce;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        await this.moveObject.DOJump(this.targetPos.position, this.jumpForce, 1, this.duration).AsyncWaitForCompletion();
    }
}