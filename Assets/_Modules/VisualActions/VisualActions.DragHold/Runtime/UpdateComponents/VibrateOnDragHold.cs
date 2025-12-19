using Lean.Touch;
using Mimi.Services.ScriptableObject.Vibration;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    public class VibrateOnDragHold : BaseUpdateProgress
    {
        [SerializeField] private BaseVibrationServiceSO vibrationService;
        private float coolDownTime;
        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            if (isIncreaseTime)
            {
                if (coolDownTime > 0)
                {
                    coolDownTime -= Time.deltaTime;
                    return;
                }

                coolDownTime = 0.1f;
                vibrationService.PlayLoopDefault();
            }
        }

        public override void CompleteProgress()
        {
         
        }

        public override void OnInit()
        {
           
        }
    }
}