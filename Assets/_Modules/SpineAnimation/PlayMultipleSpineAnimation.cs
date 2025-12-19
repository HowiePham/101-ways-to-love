using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using Spine.Unity;
using UnityEngine;

namespace VisualFlow.Spines
{
    public class PlayMultipleSpineAnimation : VisualAction
    {
        [SerializeField] private SkeletonAnimation[] skeletonAnimations;

        [SerializeField, SpineAnimation(dataField = "skeletonAnimations")]
        private new string animation;

        [SerializeField] private int track;
        [SerializeField] private bool loop;

        private bool IsAnimationComplete => this.skeletonAnimations[0].AnimationState.GetCurrent(this.track) == null ||
                                            this.skeletonAnimations[0].AnimationState.GetCurrent(this.track).IsComplete;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            foreach (SkeletonAnimation skeletonAnimation in this.skeletonAnimations)
            {
                skeletonAnimation.AnimationState.SetAnimation(this.track, this.animation, this.loop);
            }

            if (!this.loop)
            {
                try
                {
                    await UniTask.WaitUntil(() => IsAnimationComplete, PlayerLoopTiming.Update, cancellationToken);
                }
                catch (OperationCanceledException e)
                {
                }
            }
        }
    }
}