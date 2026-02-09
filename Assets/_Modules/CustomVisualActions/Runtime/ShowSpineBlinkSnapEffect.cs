using System;
using Cysharp.Threading.Tasks;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.ServiceLocators;
using Mimi.Services.ScriptableObject.Audio;
using Spine.Unity;
using UnityEngine;

public class ShowSpineBlinkSnapEffect : SnappingEffect
{
    [SerializeField] private SkeletonAnimation blinkEffect;
    [SerializeField, SoundKey] private string soundKey;
    [SerializeField] private BaseAudioServiceSO audioService;
    [SerializeField] private int track;
    [SerializeField] private float timeScale = 1f;

    [SerializeField, SpineAnimation(dataField = "blinkEffect")]
    private new string animation;

    private bool IsAnimationComplete => this.blinkEffect.AnimationState.GetCurrent(this.track) == null ||
                                        this.blinkEffect.AnimationState.GetCurrent(this.track).IsComplete;

    private void Start()
    {
        this.blinkEffect.gameObject.SetActive(false);
    }

    public override async UniTask RunEffect(Transform target)
    {
        this.blinkEffect.gameObject.SetActive(true);
        this.blinkEffect.transform.position = target.position;

        // var audioService = ServiceLocator.Global.Get<IAudioService>();
        if (this.audioService != null)
        {
            this.audioService.StopSound(this.soundKey);
            this.audioService.PlaySound(this.soundKey);
        }

        this.blinkEffect.timeScale = this.timeScale;
        this.blinkEffect.AnimationState.SetAnimation(this.track, this.animation, false);
        try
        {
            await UniTask.WaitUntil(() => IsAnimationComplete);
            this.blinkEffect.gameObject.SetActive(false);
        }
        catch (OperationCanceledException e)
        {
        }
    }
}