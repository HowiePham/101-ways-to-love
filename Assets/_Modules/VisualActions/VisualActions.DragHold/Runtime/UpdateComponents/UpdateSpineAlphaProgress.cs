using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using Mimi.VisualActions.DragHold;
using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    public class UpdateSpineAlphaProgress : BaseUpdateProgress
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private float targetAlpha;
        [SerializeField] private float initialAlpha;
        [SerializeField] private float speed = 1f;

        private Coroutine updateCoroutine;


        public override void SetTimeProgress(float timeProgress)
        {
            base.SetTimeProgress(timeProgress);
            SetAlpha(initialAlpha);
        }

        public override void OnInit()
        {

        }

        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            if (!isIncreaseTime) return;
            var currentColor = skeletonAnimation.Skeleton.A;
            currentColor = Mathf.Lerp(currentColor, progress * targetAlpha, Time.deltaTime * speed);

            SetAlpha(currentColor);

        }

        public override void CompleteProgress()
        {
            SetAlpha(targetAlpha);
        }


        public void SetTargetAlpha(float alpha)
        {
            this.targetAlpha = alpha;
        }


        public void SetAlpha(float spineAlpha)
        {

            skeletonAnimation.Skeleton.A = spineAlpha;
            foreach (var slot in skeletonAnimation.Skeleton.Slots)
            {
                slot.A = spineAlpha;
            }


        }
    }
}