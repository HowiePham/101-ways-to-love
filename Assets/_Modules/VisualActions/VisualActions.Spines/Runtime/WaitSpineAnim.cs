using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.Events;
using Sirenix.OdinInspector;
using Spine;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    [TypeInfoBox("Wait for animation completed.")]
    public class WaitSpineAnim : SpineAnimMechanic
    {
        [SerializeField] private int track;

        private bool IsAnimationComplete => this.SkeletonAnimation.AnimationState.GetCurrent(this.track) == null ||
                                            this.SkeletonAnimation.AnimationState.GetCurrent(this.track).IsComplete;


        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (!HasAnimation())
            {
                await UniTask.CompletedTask;
                return;
            }

            Messenger.Broadcast("animationstart");
            TrackEntry currentEntry = this.skeletonAnimation.AnimationState.GetCurrent(this.track);

            if (this.forcePlayAnim)
            {
                this.SkeletonAnimation.timeScale = this.timeScale;
                currentEntry = this.SkeletonAnimation.AnimationState.SetAnimation(this.track, this.Animation, false);
            }
            else
            {
                if (!currentEntry.Animation.Name.Equals(this.Animation) || currentEntry.IsComplete)
                {
                    this.SkeletonAnimation.timeScale = this.timeScale;
                    currentEntry = this.SkeletonAnimation.AnimationState.SetAnimation(this.track, this.Animation, false);
                }
            }

            try
            {
                await UniTask.WaitUntil(() => currentEntry.IsComplete,
                    PlayerLoopTiming.Update, cancellationToken);

                Messenger.Broadcast("animationcomplete");
            }
            catch (OperationCanceledException e)
            {
            }
        }
    }
}