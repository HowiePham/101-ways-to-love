using Mimi.Audio;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.Services.ScriptableObject.Audio;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class PlaySoundWhileDragging : MonoDraggableExtension
    {
        [SoundKey] [SerializeField] private string soundKey;

        [HideInBehaviourEditor] [SerializeField]
        private BaseAudioServiceSO audioService;

        public override void StartDrag()
        {
            this.audioService.PlaySound(this.soundKey);
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            this.audioService.StopSound(this.soundKey);
        }
    }
}