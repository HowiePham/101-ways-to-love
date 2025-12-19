using Lean.Touch;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    public abstract class BaseUpdateProgress : MonoBehaviour
    {
        protected float timeProgress;
        protected bool isSetup;

        /// <summary>
        /// Updates progress with given values.
        /// </summary>
        /// <param name="progress">The current progress value.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <param name="isFingerDown">Whether the finger is down.</param>
        /// <param name="isIncreaseTime">Whether time is increasing.</param>
        public abstract void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime);

        /// <summary>
        /// Completes the progress.
        /// </summary>
        public abstract void CompleteProgress();

        /// <summary>
        /// Sets the time progress value.
        /// </summary>
        /// <param name="timeProgress">The time progress to set.</param>
        public virtual void SetTimeProgress(float timeProgress)
        {
            this.timeProgress = timeProgress;
            isSetup = true;
        }

        /// <summary>
        /// Initializes the progress updater.
        /// </summary>
        public abstract void OnInit();
    }
}