using Cysharp.Threading.Tasks;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;

public class PlayAudioCheckingEffect : SnappingBoxCheckingEffect
{
    [SerializeField] private BaseAudioServiceSO audioService;
    [SerializeField, SoundKey] private string soundKey;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Range(0f, 5f)] private float pitch = 1f;
    [SerializeField] private float delaySeconds = 0f;

    public override async UniTask ShowEffect(Transform target)
    {
        this.audioService.StopSound(this.soundKey);
        this.audioService.PlaySound(this.soundKey, this.volume, this.pitch);
    }

    public override async UniTask HideEffect(Transform target)
    {
        this.audioService.StopSound(this.soundKey);
        this.audioService.PlaySound(this.soundKey, this.volume, this.pitch);
    }
}