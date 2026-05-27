using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes;
using Mimi.VisualActions;
using Spine;
using Spine.Unity;
using UnityEngine;

public class ActiveSkeletonAnimationMultiple : VisualAction
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    private Dictionary<string, string> equippedAngelSkins;

    protected override UniTask OnInitializing()
    {
        var gameContext = FindAnyObjectByType<BaseGameContext>();
        if (gameContext != null)
        {
            this.equippedAngelSkins = gameContext.GameData.EquippedAngelSkins;
        }

        return base.OnInitializing();
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        Skeleton skeleton = this.skeletonAnimation.Skeleton;
        Skin mixedSkin = new("Mix");

        foreach (string skinName in this.equippedAngelSkins.Values)
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