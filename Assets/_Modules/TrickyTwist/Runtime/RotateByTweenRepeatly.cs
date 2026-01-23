using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mimi.VisualActions;
using UnityEngine;

public class RotateByTweenRepeatly : VisualAction
{
    [SerializeField] private Transform rotateObject;
    [SerializeField] private float duration;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private bool rotateX;
    [SerializeField] private bool rotateY;
    [SerializeField] private bool rotateZ;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (!this.rotateX)
        {
            this.rotation = new Vector3(this.rotateObject.eulerAngles.x, this.rotation.y, this.rotation.z);
        }

        if (!this.rotateY)
        {
            this.rotation = new Vector3(this.rotation.x, this.rotateObject.eulerAngles.y, this.rotation.z);
        }

        if (!this.rotateZ)
        {
            this.rotation = new Vector3(this.rotation.x, this.rotation.y, this.rotateObject.eulerAngles.z);
        }

        TweenerCore<Quaternion, Vector3, QuaternionOptions> tween = this.rotateObject
            .DORotate(this.rotation, this.duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        await UniTask.Delay(TimeSpan.FromSeconds(this.duration), cancellationToken: cancellationToken);
        tween.Kill();
    }
}