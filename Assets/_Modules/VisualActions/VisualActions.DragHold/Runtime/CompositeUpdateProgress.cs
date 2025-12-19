using Lean.Touch;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    public class CompositeUpdateProgress : BaseUpdateProgress
    {
        [SerializeField] private BaseUpdateProgress[] updateProgresses;
        public override void OnInit()
        {
            foreach (var updateProgress in updateProgresses)
            {
                updateProgress.OnInit();
            }
        }

        public override void SetTimeProgress(float timeProgress)
        {
            base.SetTimeProgress(timeProgress);
            foreach (var updateProgress in updateProgresses)
            {
                updateProgress.SetTimeProgress(timeProgress);
            }
        }

        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            foreach (var updateProgress in updateProgresses)
            {
                updateProgress.OnUpdateProgress(progress, finger, isFingerDown, isIncreaseTime);   
            }
        }

        public override void CompleteProgress()
        {
            foreach (var updateProgress in updateProgresses)
            {
                updateProgress.CompleteProgress();
            }
        }
    }
}