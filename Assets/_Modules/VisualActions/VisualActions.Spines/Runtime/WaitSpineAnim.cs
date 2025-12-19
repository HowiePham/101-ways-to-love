using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    [TypeInfoBox("Wait for animation completed.")]
    public class WaitSpineAnim : VisualAction
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        [SerializeField, SpineAnimation(dataField = "skeletonAnimation")]
        private new string animation;

        [SerializeField] private int track;

        private bool IsAnimationComplete => this.skeletonAnimation.AnimationState.GetCurrent(this.track) == null ||
                                            this.skeletonAnimation.AnimationState.GetCurrent(this.track).IsComplete;


        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.skeletonAnimation.AnimationState.SetAnimation(this.track, this.animation, false);
            try
            {
                await UniTask.WaitUntil(() => IsAnimationComplete,
                    PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
        }
    }
}