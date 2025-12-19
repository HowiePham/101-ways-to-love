using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using Mimi.VisualActions.Attribute;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Deleting
{
    [TypeInfoBox("Extension to play sound when delete progress changes significantly.")]
    public class PlaySoundOnDeletingExtension : MonoSmoothDeleteExtension
    {
        [HideInBehaviourEditor]
        [SerializeField] private BaseAudioServiceSO audioServiceSO;
        [SerializeField] [SoundKey] private string soundKey;
        private float oldProgress;
        
        /// <summary>
        /// Records initial progress on delete start.
        /// </summary>
        /// <param name="position">The position where deletion starts.</param>
        public override void OnStartDelete(Vector3 position)
        {
            oldProgress = this.smoothDelete.DeleteProgress;
        }

        /// <summary>
        /// Plays sound if progress changes significantly.
        /// </summary>
        /// <param name="position">The current deletion position.</param>
        public override void OnDelete(Vector3 position)
        {
            var currentProgress = this.smoothDelete.DeleteProgress;
            if (Mathf.Abs(currentProgress - oldProgress) > 0.01f)
            {
                oldProgress = currentProgress;
                audioServiceSO.PlaySound(this.soundKey);
            }
        }

        /// <summary>
        /// Does nothing on delete stop.
        /// </summary>
        public override void OnStopDelete()
        {
        }
    }
}