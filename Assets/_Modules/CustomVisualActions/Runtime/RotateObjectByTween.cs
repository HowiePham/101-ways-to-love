using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;

public class RotateObjectByTween : VisualAction
{
    [SerializeField] private Transform rotateObject;
    [SerializeField] private Vector3 rotateAxis;
    [SerializeField] private float duration;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private bool completeAfterRotating = true;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.completeAfterRotating)
        {
            await this.rotateObject.DORotate(this.rotateAxis, this.duration).SetEase(this.ease).AsyncWaitForCompletion();
        }
        else
        {
            this.rotateObject.DORotate(this.rotateAxis, this.duration).SetEase(this.ease);
        }

        await UniTask.CompletedTask;
    }
}