using System;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    [TypeInfoBox("Updates GameObject active status based on progress.")]
    public class UpdateGameObjectStatusProgress : BaseUpdateProgress
    {
        [SerializeField] private GameObject gameObject;
        [SerializeField] private bool targetStatus;
        private float countDownTime = 0.1f;
        private float currentCountDownTime;

        /// <summary>
        /// Updates countdown and deactivates object if countdown expires.
        /// </summary>
        private void Update()
        {
            if (currentCountDownTime > 0)
            {
                currentCountDownTime -= Time.deltaTime;
            }
            else
            {
                gameObject.SetActive(!targetStatus);
            }
        }

        /// <summary>
        /// Sets object active and resets countdown during progress.
        /// </summary>
        /// <param name="progress">The current progress value.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <param name="isFingerDown">Whether the finger is down.</param>
        /// <param name="isIncreaseTime">Whether time is increasing.</param>
        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            if (isIncreaseTime)
            {
                gameObject.SetActive(targetStatus);
                currentCountDownTime = countDownTime;
            }
        }

        /// <summary>
        /// Completes the progress (no action).
        /// </summary>
        public override void CompleteProgress()
        {

        }

        /// <summary>
        /// Initializes the progress updater.
        /// </summary>
        public override void OnInit()
        {
            
        }
    }
}