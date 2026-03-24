using Mimi.Audio;
using UnityEngine;

namespace Mimi.Prototypes
{
    public class AudioServiceAdapter : IAudioService
    {
        public float SoundVolPercentage => this.audioPlayer.SoundVolPercentage;
        public float MusicVolPercentage => this.audioPlayer.MusicVolPercentage;
        protected float soundVolPercentage = 1f;
        protected float musicVolPercentage = 1f;

        private readonly IAudioPlayer audioPlayer;

        public AudioServiceAdapter(IAudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
        }

        public void PlaySound(string key, float volumePercentage = 1, float pitch = 1)
        {
            this.audioPlayer.PlaySound(key, volumePercentage, pitch);
            SetSoundVolPercentage(this.soundVolPercentage);
            SetMusicVolPercentage(this.musicVolPercentage);
        }

        public void SetSoundVolPercentage(float percentage)
        {
            this.audioPlayer.SetSoundVolPercentage(percentage);
            this.soundVolPercentage = percentage;

            // Debug.Log($"--- (AUDIO) New sound vol: {this.soundVolPercentage}");
        }

        public void SetMusicVolPercentage(float percentage)
        {
            this.audioPlayer.SetMusicVolPercentage(percentage);
            this.musicVolPercentage = percentage;

            // Debug.Log($"--- (AUDIO) New Music vol: {this.musicVolPercentage}");
        }
    }
}