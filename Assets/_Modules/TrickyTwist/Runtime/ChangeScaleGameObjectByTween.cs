using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;

public class ChangeScaleGameObjectByTween : VisualAction
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private float valueChange;
    [SerializeField] private float duration;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        Vector3 newScale = this.targetObject.localScale + new Vector3(this.valueChange, this.valueChange, this.valueChange);
        await this.targetObject.DOScale(newScale, this.duration).AsyncWaitForCompletion();
    }
}