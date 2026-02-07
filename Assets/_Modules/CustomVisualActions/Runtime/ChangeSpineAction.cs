using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using Mimi.VisualActions.Spines;
using Spine.Unity;
using UnityEngine;

public class ChangeSpineAction : VisualAction
{
    [SerializeField] private SpineAnimMechanic spineAnimMechanic;
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [SerializeField, SpineAnimation(dataField = "skeletonAnimation")]
    private new string animation;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.spineAnimMechanic == null || this.skeletonAnimation == null || string.IsNullOrEmpty(this.animation))
        {
            await UniTask.CompletedTask;
            return;
        }

        this.spineAnimMechanic.SetAnimationAction(this.animation);
        await UniTask.CompletedTask;
    }
}