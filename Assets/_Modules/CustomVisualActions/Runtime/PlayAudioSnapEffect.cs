using Cysharp.Threading.Tasks;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.ServiceLocators;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;

public class PlayAudioSnapEffect : SnappingEffect
{
    [SerializeField, SoundKey] private string soundKey;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Range(0f, 5f)] private float pitch = 1f;
    [SerializeField] private float delaySeconds = 0f;
    [SerializeField] private bool playSound = true;

    [SerializeField] private BaseAudioServiceSO audioPlayer;

    public override async UniTask RunEffect(Transform target)
    {
        this.audioPlayer.StopSound(this.soundKey);
        var audioService = ServiceLocator.Global.Get<IAudioService>();
        
        if (audioService == null || string.IsNullOrEmpty(this.soundKey))
        {
            return;
        }

        audioService.PlaySound(this.soundKey, this.volume, this.pitch);
    }
}