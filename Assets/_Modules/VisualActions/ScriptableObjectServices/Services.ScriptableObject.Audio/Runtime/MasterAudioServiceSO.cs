using Cysharp.Threading.Tasks;
using Mimi.Audio;
using Mimi.Audio.MasterAudios;
using UnityEngine;

namespace Mimi.Services.ScriptableObject.Audio
{
    [CreateAssetMenu(menuName = "ServicesSO/Audio/MasterAudioServiceSO")]
    public class MasterAudioServiceSO : BaseAudioServiceSO
    {
        protected override IAudioPlayer WrapAudioPlayer
        {
            get
            {
                if (wrapAudioPlayer == null)
                {
                    Initialize();
                }
                return wrapAudioPlayer;
            }
        }

        public override void Dispose()
        {
        }
        protected override IAudioPlayer CreateAudioPlayer()
        {
            return new MasterAudioPlayer();
        }

        public override void StopSound(string key)
        {
            (WrapAudioPlayer as MasterAudioPlayer)?.StopSound(key);
        }
    }
}