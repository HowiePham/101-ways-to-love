using System;
using Lean.Touch;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    [TypeInfoBox("Plays Spine animation based on progress.")]
    public class UpdateAnimationProgress : BaseUpdateProgress
    {
        [SerializeField] private SkeletonAnimation animation;

        [SerializeField, SpineAnimation(dataField: "animation")]
        private string animationName;

        [SerializeField] private float initialFrame;
        [SerializeField] private bool isNormalSpeed;

        [SerializeField, ShowIf("isNormalSpeed")]
        private float normalSpeedAnim = 1f;

        [SerializeField] private bool isLooping;
        private bool lastStatus;
        private float speedAnimation;
        private float timeAnim = 0.1f;
        private float currentTimeAnim;
        private bool isComplete;

        /// <summary>
        /// Updates animation timescale based on countdown.
        /// </summary>
        private void FixedUpdate()
        {
            if (isComplete || !isSetup) return;
            if (currentTimeAnim > 0)
            {
                currentTimeAnim -= Time.deltaTime;
            }
            else if (this.animation.timeScale > 0.1f)
            {
                this.animation.timeScale = 0f;
            }
        }

        /// <summary>
        /// Initializes animation timescale to zero.
        /// </summary>
        public override void OnInit()
        {
            this.animation.timeScale = 0f;
        }

        /// <summary>
        /// Sets time progress and initializes animation.
        /// </summary>
        /// <param name="timeProgress">The time progress to set.</param>
        public override void SetTimeProgress(float timeProgress)
        {
            base.SetTimeProgress(timeProgress);
            float animationLength = this.animation.skeleton.Data.FindAnimation(animationName).Duration;
            speedAnimation = animationLength / timeProgress;
            this.animation.AnimationState.SetAnimation(0, this.animationName, isLooping);
            this.animation.timeScale = 0f;
            this.animation.AnimationState.Update(initialFrame);
            this.animation.AnimationState.Apply(this.animation.skeleton);
            isComplete = false;
            if (isNormalSpeed)
            {
                speedAnimation = normalSpeedAnim;
            }
        }

        /// <summary>
        /// Updates animation timescale based on progress.
        /// </summary>
        /// <param name="progress">The current progress value.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <param name="isFingerDown">Whether the finger is down.</param>
        /// <param name="isIncreaseTime">Whether time is increasing.</param>
        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            if (currentTimeAnim > 0)
            {
                currentTimeAnim -= Time.deltaTime;
            }

            if (currentTimeAnim > 0) return;
            if (isIncreaseTime)
            {
                this.animation.timeScale = speedAnimation;
                currentTimeAnim = timeAnim;
            }
            else
            {
                this.animation.timeScale = 0f;
            }
        }

        /// <summary>
        /// Sets animation to play at speed on completion.
        /// </summary>
        public override void CompleteProgress()
        {
            isComplete = true;
            this.animation.timeScale = speedAnimation;
        }
    }
}