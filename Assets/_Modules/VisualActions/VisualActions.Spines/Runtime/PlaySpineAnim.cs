using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    [TypeInfoBox("Play an animation fire and forget.")]
    public class PlaySpineAnim : VisualAction
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        [SerializeField, SpineAnimation(dataField = "skeletonAnimation")]
        private new string animation;

        [SerializeField] private int track;
        [SerializeField] private bool loop;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.skeletonAnimation.AnimationState.SetAnimation(this.track, this.animation, this.loop);
            await UniTask.CompletedTask;
        }
    }
}