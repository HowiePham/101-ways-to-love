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
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private bool turnOffObjectAfterJumping;
    [SerializeField] private bool completeAfterMove;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.completeAfterMove)
        {
            await this.moveObject.DOJump(this.targetPos.position, this.jumpForce, 1, this.duration).SetEase(this.ease).AsyncWaitForCompletion();
            if (this.turnOffObjectAfterJumping)
            {
                DisableObject();
            }
        }
        else
        {
            this.moveObject.DOJump(this.targetPos.position, this.jumpForce, 1, this.duration).SetEase(this.ease);
            Invoke(nameof(DisableObject), this.duration);
        }


        await UniTask.CompletedTask;
    }

    private void DisableObject()
    {
        this.moveObject.gameObject.SetActive(false);
    }
}