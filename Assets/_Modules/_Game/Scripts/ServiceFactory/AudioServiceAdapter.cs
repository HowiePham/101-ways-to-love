using Mimi.Audio;

namespace Mimi.Prototypes
{
    public class AudioServiceAdapter : IAudioService
    {
        public float SoundVolPercentage => this.audioPlayer.SoundVolPercentage;
        public float MusicVolPercentage => this.audioPlayer.MusicVolPercentage;

        private readonly IAudioPlayer audioPlayer;

        public AudioServiceAdapter(IAudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
        }

        public void PlaySound(string key, float volumePercentage = 1, float pitch = 1)
        {
            this.audioPlayer.PlaySound(key, volumePercentage, pitch);
        }

        public void SetSoundVolPercentage(float percentage)
        {
            this.audioPlayer.SetSoundVolPercentage(percentage);
        }

        public void SetMusicVolPercentage(float percentage)
        {
            this.audioPlayer.SetMusicVolPercentage(percentage);
        }
    }
}