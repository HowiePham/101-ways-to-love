using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class PlaySoundOnPickUpExtension : MonoItemDragExtension
    {
        [SoundKey]
        [SerializeField] private string soundKey;
        [SerializeField] private BaseAudioServiceSO audioService;
        public override void OnInit(DraggableItem draggableItem)
        {
            
        }

        public override void OnPickUp(DraggableItem draggableItem)
        {
           audioService.PlaySound(soundKey);
        }

        public override void OnReturn(DraggableItem draggableItem)
        {
           
        }
    }
}