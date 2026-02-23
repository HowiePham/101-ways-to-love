using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using Spine.Unity;
using UnityEngine;

public class ChangeAnimationSkin : VisualAction
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [SerializeField, SpineSkin(dataField = "skeletonAnimation")]
    private string skin;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.skeletonAnimation.skeleton.SetSkin(this.skin);
        this.skeletonAnimation.skeleton.SetToSetupPose();
        await UniTask.CompletedTask;
    }
}