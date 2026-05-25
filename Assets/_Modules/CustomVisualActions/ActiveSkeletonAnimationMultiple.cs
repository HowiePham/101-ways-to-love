using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using Spine;
using Spine.Unity;
using UnityEngine;

public class ActiveSkeletonAnimationMultiple : VisualAction
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [SerializeField, SpineSkin(dataField = "skeletonAnimation")]
    private string[] skins;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        Skeleton skeleton = this.skeletonAnimation.Skeleton;
        Skin mixedSkin = new("Mix");

        foreach (string skinName in this.skins)
        {
            Skin skin = skeleton.Data.FindSkin(skinName);
            if (skin != null)
                mixedSkin.AddSkin(skin);
        }

        skeleton.SetSkin(mixedSkin);
        skeleton.SetSlotsToSetupPose();
        this.skeletonAnimation.AnimationState.Apply(skeleton);

        await UniTask.CompletedTask;
    }
}
