using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Mimi.VisualActions.DragHold
{
    [TypeInfoBox("Updates image fill amount based on progress.")]
    public class UpdateFillAmountImageProgress : BaseUpdateProgress
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private float maxFillAmount = 1f;
        [SerializeField] private float initialFillAmount = 0f;

        /// <summary>
        /// Updates image fill amount during progress.
        /// </summary>
        /// <param name="progress">The current progress value.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <param name="isFingerDown">Whether the finger is down.</param>
        /// <param name="isIncreaseTime">Whether time is increasing.</param>
        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            if (!isIncreaseTime) return;
            targetImage.fillAmount = initialFillAmount + progress * maxFillAmount;
        }

        /// <summary>
        /// Sets image fill amount to maximum on completion.
        /// </summary>
        public override void CompleteProgress()
        {
            targetImage.fillAmount = initialFillAmount + maxFillAmount;
        }

        /// <summary>
        /// Initializes the progress updater.
        /// </summary>
        public override void OnInit()
        {
            
        }
    }
}