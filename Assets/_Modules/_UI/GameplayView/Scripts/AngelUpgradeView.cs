using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Prototypes.UI;
using Mimi.VisualActions.Spines;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;
using AnimationState = Spine.AnimationState;
using Event = Spine.Event;

public class AngelUpgradeView : BaseView
{
    [SerializeField] private SkeletonGraphic angelSkeletonGraphic;
    [SerializeField] private CanvasGroup darkBG;

    [Header("Animation Settings")] [SerializeField]
    private float bgFadeDuration = 0.3f;

    [SerializeField] private float bgTargetAlpha = 0.95f;
    [SerializeField] private float idleDuration = 2f;
    [SerializeField] private float happyDuration = 2f;

    [SpineEvent(dataField: "angelSkeletonGraphic", fallbackToTextField: true)]
    public string skinChangedEvent;

    [SerializeField, SpineAnimation(dataField = "angelSkeletonGraphic")]
    protected string appearingAnimation;

    [SerializeField, SpineAnimation(dataField = "angelSkeletonGraphic")]
    protected string changeSkinAnimation;

    [SerializeField, SpineAnimation(dataField = "angelSkeletonGraphic")]
    protected string idleAnimation;

    [SerializeField, SpineAnimation(dataField = "angelSkeletonGraphic")]
    protected string happyAnimation;

    [SerializeField, SpineAnimation(dataField = "angelSkeletonGraphic")]
    protected string disappearingAnimation;

    [SerializeField, SpineAnimation(dataField = "angelSkeletonGraphic")]
    protected string disappearingLoopAnimation;

    private CancellationTokenSource sequenceCts;
    private List<string> newAngelSkins;
    private bool appliedNewSkin;
    public Action OnTestingAngelUpgradingEffect;

    public override void Initialize()
    {
        base.Initialize();
        this.darkBG.alpha = 0f;
    }

    public override void Hide()
    {
        AnimationState animState = this.angelSkeletonGraphic.AnimationState;
        this.angelSkeletonGraphic.PlayAnimation(this.disappearingLoopAnimation, true);
        if (animState != null)
        {
            animState.Event -= ApplyNewAngelSkin;
        }

        this.angelSkeletonGraphic.gameObject.SetActive(false);
        this.sequenceCts?.Cancel();
        this.sequenceCts?.Dispose();
        this.sequenceCts = null;
        DOTween.Kill(this.darkBG);
        this.darkBG.alpha = 0f;
        this.darkBG.gameObject.SetActive(false);
        base.Hide();
    }

    public async UniTask PlaySequenceAsync(List<string> oldAngelSkin, List<string> newAngelSkin, CancellationToken externalCt)
    {
        this.sequenceCts?.Cancel();
        this.sequenceCts?.Dispose();
        this.sequenceCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        var ct = this.sequenceCts.Token;

        this.appliedNewSkin = false;
        this.newAngelSkins = newAngelSkin;
        this.darkBG.gameObject.SetActive(true);
        this.darkBG.alpha = 0f;

        await this.darkBG.DOFade(this.bgTargetAlpha, this.bgFadeDuration).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        this.angelSkeletonGraphic.gameObject.SetActive(true);

        AnimationState animState = this.angelSkeletonGraphic.AnimationState;
        if (animState != null)
        {
            animState.Event += ApplyNewAngelSkin;
        }

        ApplyMixedSkin(oldAngelSkin);

        await this.angelSkeletonGraphic.WaitAnimation(this.appearingAnimation, cancellationToken: ct);
        if (ct.IsCancellationRequested) return;

        this.angelSkeletonGraphic.PlayAnimation(this.idleAnimation, true);
        bool cancelled = await UniTask.WaitForSeconds(this.idleDuration, cancellationToken: ct).SuppressCancellationThrow();
        if (cancelled) return;

        await this.angelSkeletonGraphic.WaitAnimation(this.changeSkinAnimation, cancellationToken: ct);
        if (ct.IsCancellationRequested) return;

        this.angelSkeletonGraphic.PlayAnimation(this.happyAnimation, true);
        cancelled = await UniTask.WaitForSeconds(this.happyDuration, cancellationToken: ct).SuppressCancellationThrow();
        if (cancelled) return;

        // this.darkBG.DOFade(0f, this.bgFadeDuration);
        await this.angelSkeletonGraphic.WaitAnimation(this.disappearingAnimation, cancellationToken: ct);
    }

    private void ApplyNewAngelSkin(TrackEntry trackEntry, Event e)
    {
        bool eventMatch = string.Equals(e.Data.Name, this.skinChangedEvent, StringComparison.Ordinal);
        if (!eventMatch || this.appliedNewSkin) return;

        ApplyMixedSkin(this.newAngelSkins);
        this.appliedNewSkin = true;
    }

    [Button]
    private void TestAngelUpgradeEffect()
    {
        OnTestingAngelUpgradingEffect?.Invoke();
    }

    private void ApplyMixedSkin(List<string> skinNames)
    {
        if (skinNames == null || skinNames.Count == 0) return;
        Skeleton skeleton = this.angelSkeletonGraphic.Skeleton;
        Skin mixedSkin = new Skin("Mix");
        foreach (string name in skinNames)
        {
            Skin skin = skeleton.Data.FindSkin(name);
            if (skin != null) mixedSkin.AddSkin(skin);
        }

        skeleton.SetSkin(mixedSkin);
        skeleton.SetSlotsToSetupPose();
        this.angelSkeletonGraphic.AnimationState.Apply(skeleton);
    }

    private void ApplySingleSkin(string skinName)
    {
        if (string.IsNullOrEmpty(skinName)) return;
        Skeleton skeleton = this.angelSkeletonGraphic.Skeleton;
        Skin skin = skeleton.Data.FindSkin(skinName);
        if (skin == null) return;
        skeleton.SetSkin(skin);
        skeleton.SetSlotsToSetupPose();
        this.angelSkeletonGraphic.AnimationState.Apply(skeleton);
    }
}