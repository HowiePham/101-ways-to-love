using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;

namespace Mimi.VisualActions.BookFlip
{
    public class PlaySoundOnStartBookFlipExtension : MonoBookFlipExtension
    {
        [SoundKey]
        [SerializeField] private string soundKey;
        [SerializeField] private BaseAudioServiceSO audioService;
        public override void Start()
        {
            
        }

        public override void FlipStart()
        {
           audioService.PlaySound(soundKey);
        }

        public override void FlipCompleted()
        {
           
        }

        public override void PageRelease()
        {
            
        }
    }
}