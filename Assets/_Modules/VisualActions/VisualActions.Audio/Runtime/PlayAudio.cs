using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;

namespace Mimi.VisualActions.Audio
{
    public class PlayAudio : VisualAction
    {
        [SerializeField, SoundKey] private string soundKey;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(0f, 5f)] private float pitch = 1f;
        [SerializeField] private float delaySeconds = 0f;
        [SerializeField] private bool playSound = true;

        [SerializeField] private BaseAudioServiceSO audioPlayer;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            Invoke(nameof(PlaySound), delaySeconds);
        }

        void PlaySound()
        {
            if (this.playSound)
            {
                this.audioPlayer.PlaySound(this.soundKey, this.volume, this.pitch);
            }
            else
            {
                this.audioPlayer.StopSound(this.soundKey);
            }
        }
    }
}