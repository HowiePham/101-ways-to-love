using Cysharp.Threading.Tasks;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Core;
using UnityEngine;

namespace Mimi.Services.ScriptableObject.Audio
{
    public abstract class BaseAudioServiceSO : BaseServiceSO, IAudioPlayer
    {
        protected IAudioPlayer wrapAudioPlayer;
        protected virtual IAudioPlayer WrapAudioPlayer => wrapAudioPlayer;
        protected float soundVolPercentage = 1f;
        protected float musicVolPercentage = 1f;

        public override UniTask Initialize()
        {
            wrapAudioPlayer = CreateAudioPlayer();
            return UniTask.CompletedTask;
        }

        protected abstract IAudioPlayer CreateAudioPlayer();

        public void PlaySound(string key, float volumePercentage = 1, float pitch = 1)
        {
            WrapAudioPlayer?.PlaySound(key, volumePercentage, pitch);
            SetSoundVolPercentage(this.soundVolPercentage);
            SetMusicVolPercentage(this.musicVolPercentage);
        }

        public void SetSoundVolPercentage(float percentage)
        {
            WrapAudioPlayer?.SetSoundVolPercentage(percentage);
            this.soundVolPercentage = percentage;
        }

        public void SetMusicVolPercentage(float percentage)
        {
            WrapAudioPlayer?.SetMusicVolPercentage(percentage);
            this.musicVolPercentage = percentage;
        }

        public abstract void StopSound(string key);
        public float SoundVolPercentage => WrapAudioPlayer?.SoundVolPercentage ?? 0f;
        public float MusicVolPercentage => WrapAudioPlayer?.MusicVolPercentage ?? 0f;
    }
}