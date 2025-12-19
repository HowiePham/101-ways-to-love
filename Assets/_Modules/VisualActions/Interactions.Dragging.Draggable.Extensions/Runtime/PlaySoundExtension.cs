using Mimi.Audio;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.Services.ScriptableObject.Audio;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class PlaySoundExtension : MonoDraggableExtension
    {
        [SoundKey]
        [SerializeField] private string selectSound;
        
        [SoundKey]
        [SerializeField] private string deselectSound;
        
        [HideInBehaviourEditor]
        [SerializeField] private BaseAudioServiceSO audioService;
        
        public override void StartDrag()
        {
            Debug.Log($"--- (DRAG) Start audio dragging");
            audioService.PlaySound(selectSound);
        }

        public override void Drag()
        {
     
        }

        public override void EndDrag()
        {
            audioService.PlaySound(deselectSound);
        }
    }
}