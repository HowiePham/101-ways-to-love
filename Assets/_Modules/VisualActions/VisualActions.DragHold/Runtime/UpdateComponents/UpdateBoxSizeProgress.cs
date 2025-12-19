using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.DragHold
{
    [TypeInfoBox("Updates box area size based on progress.")]
    public class UpdateBoxSizeProgress : BaseUpdateProgress
    {
        [SerializeField] private BoxArea boxArea;
        [SerializeField] private Vector2 startSize;
        [SerializeField] private Vector2 endSize;

        private float speed = 5f;

        /// <summary>
        /// Updates box size during progress.
        /// </summary>
        /// <param name="progress">The current progress value.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <param name="isFingerDown">Whether the finger is down.</param>
        /// <param name="isIncreaseTime">Whether time is increasing.</param>
        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            if (!isIncreaseTime) return;
            var targetSize = startSize + (endSize - startSize) * progress;
            boxArea.SetSize(targetSize);
        }

        /// <summary>
        /// Sets box size to end size on completion.
        /// </summary>
        public override void CompleteProgress()
        {
            boxArea.SetSize(endSize);
        }

        /// <summary>
        /// Initializes the progress updater.
        /// </summary>
        public override void OnInit()
        {
            
        }
    }
}