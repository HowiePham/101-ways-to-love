using Cysharp.Threading.Tasks;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Core;

namespace Mimi.Services.ScriptableObject.Audio
{
    public abstract class BaseAudioServiceSO : BaseServiceSO, IAudioPlayer
    {
        protected IAudioPlayer wrapAudioPlayer;
        protected virtual IAudioPlayer WrapAudioPlayer => wrapAudioPlayer;

        public override UniTask Initialize()
        {
            wrapAudioPlayer = CreateAudioPlayer();
            return UniTask.CompletedTask;
        }
        protected abstract IAudioPlayer CreateAudioPlayer();

        public void PlaySound(string key, float volumePercentage = 1, float pitch = 1)
        {
            WrapAudioPlayer?.PlaySound(key, volumePercentage, pitch);
        }

        public void SetSoundVolPercentage(float percentage)
        {
            WrapAudioPlayer?.SetSoundVolPercentage(percentage);
        }

        public void SetMusicVolPercentage(float percentage)
        {
            WrapAudioPlayer?.SetMusicVolPercentage(percentage);
        }

        public abstract void StopSound(string key);
        public float SoundVolPercentage => WrapAudioPlayer?.SoundVolPercentage ?? 0f;
        public float MusicVolPercentage => WrapAudioPlayer?.MusicVolPercentage ?? 0f;
    }
}