using Lean.Touch;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    public class PlaySoundOnDragHold : BaseUpdateProgress
    {
        [SoundKey]
        [SerializeField] private string soundKey;
        [SerializeField] private BaseAudioServiceSO audioService;

       
        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            if (isIncreaseTime)
            {
                audioService.PlaySound(soundKey);
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