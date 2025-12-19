using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.MultipleTapping.Extensions
{
    [Serializable]
    public class AnimationTimelineData
    {
        public float timeEventStart;
        
        /// <summary>
        /// Converts start time to milliseconds.
        /// </summary>
        /// <returns>The start time in milliseconds.</returns>
        public int GetTimeMiliseconds()
        {
            return (int)(timeEventStart * 1000f);
        }
    }

    [TypeInfoBox("Advances a Spine animation timeline based on tap progress.")]
    public class MultipleTappingPlayAnimationMultiStage : MonoMultipleTappingExtension
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField, SpineAnimation(dataField: "skeletonAnimation")] private string animationName;
        [SerializeField] private List<AnimationTimelineData> animationTimelineData;
        [SerializeField] private float speedAnim = 1f;
        [SerializeField] private MultipleTappingAction multipleTappingAction;

        /// <summary>
        /// Initializes the extension (no action required).
        /// </summary>
        public override void Init()
        {
        }

        /// <summary>
        /// Sets initial animation state and pauses it.
        /// </summary>
        public override void Start()
        {
            skeletonAnimation.AnimationState.SetAnimation(0, animationName, false);
            skeletonAnimation.AnimationState.GetCurrent(0).TrackTime = animationTimelineData[0].timeEventStart;
            skeletonAnimation.timeScale = 0f;
        }

        /// <summary>
        /// Advances animation to specified timeline point on tap.
        /// </summary>
        /// <param name="currentStep">The current tap step.</param>
        /// <param name="totalSteps">The total number of tap steps.</param>
        /// <returns>UniTask representing async completion.</returns>
        public override async UniTask OnTap(int currentStep, int totalSteps)
        {
            skeletonAnimation.timeScale = speedAnim;
            float currentTimeData = currentStep < this.animationTimelineData.Count ? 
                animationTimelineData[currentStep].timeEventStart : GetClipLength();
            await UniTask.WaitUntil(() => skeletonAnimation.AnimationState.GetCurrent(0).TrackTime >= currentTimeData);
            if (multipleTappingAction.CurrentStep <= currentStep)
            {
                skeletonAnimation.timeScale = 0f;
            }
        }

        /// <summary>
        /// Gets the duration of the animation clip.
        /// </summary>
        /// <returns>The animation duration in seconds.</returns>
        private float GetClipLength()
        {
            return skeletonAnimation.Skeleton.Data.FindAnimation(animationName).Duration;
        }
    }
}