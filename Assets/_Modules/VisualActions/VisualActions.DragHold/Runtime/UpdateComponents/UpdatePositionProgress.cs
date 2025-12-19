using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    [TypeInfoBox("Updates position from start to target based on progress.")]
    public class UpdatePositionProgress : BaseUpdateProgress
    {
        [SerializeField] private Transform from;
        [SerializeField] private Transform target;
        [SerializeField] private float speed = 5f;

        /// <summary>
        /// Updates position during progress.
        /// </summary>
        /// <param name="progress">The current progress value.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <param name="isFingerDown">Whether the finger is down.</param>
        /// <param name="isIncreaseTime">Whether time is increasing.</param>
        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            if (!isIncreaseTime) return;
            var targetPOs = from.position + (target.position - from.position) * progress;
            from.transform.position = Vector3.Lerp(from.transform.position, targetPOs, speed * Time.deltaTime);
        }

        /// <summary>
        /// Sets position to target on completion.
        /// </summary>
        public override void CompleteProgress()
        {
            from.transform.position = target.position;
        }

        /// <summary>
        /// Initializes the progress updater.
        /// </summary>
        public override void OnInit()
        {
            
        }
    }
}