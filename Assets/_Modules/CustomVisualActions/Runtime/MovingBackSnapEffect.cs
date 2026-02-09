using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class MovingBackSnapEffect : SnappingEffect
{
    [SerializeField] private Vector3 startPosOffset = Vector3.zero;
    [SerializeField] private float duration;
    [SerializeField] private Ease ease;

    public override async UniTask RunEffect(Transform target)
    {
        if (this.startPosOffset == Vector3.zero)
        {
            return;
        }

        Vector3 originalPos = target.position;

        var startPos = new Vector3(originalPos.x + this.startPosOffset.x, originalPos.y + this.startPosOffset.y, originalPos.z + this.startPosOffset.z);
        target.position = startPos;

        if (this.waitEffect)
        {
            await target.DOMove(originalPos, this.duration).SetEase(this.ease).AsyncWaitForCompletion();
        }
        else
        {
            target.DOMove(originalPos, this.duration).SetEase(this.ease);
        }
    }
}