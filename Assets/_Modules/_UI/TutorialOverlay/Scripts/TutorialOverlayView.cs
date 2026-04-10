using System;
using System.Threading;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialOverlayView : BaseView
{
    [Header("Overlay")]
    [SerializeField] private Image darkOverlayImage;
    [SerializeField] private RectTransform spotlightRect;
    [SerializeField] private Unmask unmask;

    [Header("Tooltip")]
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private CanvasGroup tooltipCanvasGroup;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("Step Descriptions")]
    [SerializeField] private TMP_Text[] stepDescriptions;

    [Header("Container")]
    [SerializeField] private GameObject tutorialRoot;

    [Header("Steps")]
    [SerializeField] private TutorialStepData[] steps;

    [Header("Animation Settings")]
    [SerializeField] private float overlayFadeDuration = 0.35f;
    [SerializeField] private float overlayTargetAlpha = 0.75f;
    [SerializeField] private float tooltipFadeDuration = 0.25f;
    [SerializeField] private float spotlightPunchScale = 1.15f;
    [SerializeField] private float spotlightScaleDuration = 0.3f;
    [SerializeField] private float descriptionFadeDuration = 0.3f;
    [SerializeField] private string nextButtonLabelNext = "Tap to Continue";
    [SerializeField] private string nextButtonLabelFinish = "Got it!";

    public Action OnNextClicked;

    public TutorialStepData[] Steps => this.steps;

    public override void Initialize()
    {
        base.Initialize();
        this.darkOverlayImage.color = new Color(0f, 0f, 0f, 0f);
        this.tooltipCanvasGroup.alpha = 0f;
        this.spotlightRect.localScale = Vector3.zero;
        HideAllDescriptions();
        this.nextButton.onClick.AddListener(() => OnNextClicked?.Invoke());
    }

    public override void Show()
    {
        // Do NOT call base.Show() — it disables the shared gameplay Canvas
        this.tutorialRoot.SetActive(true);
        this.darkOverlayImage.color = new Color(0f, 0f, 0f, 0f);
        this.tooltipCanvasGroup.alpha = 0f;
        this.spotlightRect.localScale = Vector3.zero;
        HideAllDescriptions();
    }

    public override void Hide()
    {
        KillAllTweens();
        this.darkOverlayImage.color = new Color(0f, 0f, 0f, 0f);
        this.tooltipCanvasGroup.alpha = 0f;
        this.spotlightRect.localScale = Vector3.zero;
        HideAllDescriptions();
        // Do NOT call base.Hide() — it disables the shared gameplay Canvas
        this.tutorialRoot.SetActive(false);
    }

    public async UniTask PlayIntroAnimation(CancellationToken ct)
    {
        await this.darkOverlayImage
            .DOFade(this.overlayTargetAlpha, this.overlayFadeDuration)
            .SetEase(Ease.OutCubic)
            .AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(ct)
            .SuppressCancellationThrow();
    }

    public async UniTask TransitionToStep(TutorialStepData stepData, RectTransform targetRect, int stepIndex, CancellationToken ct)
    {
        // 1. Fade out tooltip + hide current description
        await FadeTooltip(false, ct);
        if (ct.IsCancellationRequested) return;

        // 2. Reposition and animate spotlight
        if (targetRect != null)
        {
            await AnimateSpotlightToTarget(targetRect, stepData.spotlightPadding, ct);
        }
        else
        {
            await this.spotlightRect
                .DOScale(0f, this.spotlightScaleDuration)
                .SetEase(Ease.InBack)
                .AsyncWaitForCompletion()
                .AsUniTask()
                .AttachExternalCancellation(ct)
                .SuppressCancellationThrow();
        }

        if (ct.IsCancellationRequested) return;

        // 3. Show this step's description text with fade-in effect
        ShowStepDescription(stepIndex, ct).Forget();

        // 4. Update next button label and fade in tooltip
        this.nextButtonText.text = stepData.isLastStep ? this.nextButtonLabelFinish : this.nextButtonLabelNext;
        await FadeTooltip(true, ct);
    }

    public async UniTask PlayOutroAnimation(CancellationToken ct)
    {
        KillAllTweens();
        var seq = DOTween.Sequence();
        seq.Join(this.tooltipCanvasGroup.DOFade(0f, this.tooltipFadeDuration));
        seq.Join(this.spotlightRect.DOScale(0f, this.spotlightScaleDuration).SetEase(Ease.InBack));
        seq.Append(this.darkOverlayImage.DOFade(0f, this.overlayFadeDuration).SetEase(Ease.InCubic));

        await seq.AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(ct)
            .SuppressCancellationThrow();
    }

    private async UniTask AnimateSpotlightToTarget(RectTransform target, float padding, CancellationToken ct)
    {
        this.unmask.FitTo(target);
        this.spotlightRect.sizeDelta = target.rect.size + Vector2.one * (padding * 2f);
        this.spotlightRect.localScale = Vector3.zero;

        float punchDuration = this.spotlightScaleDuration * 0.7f;
        float settleDuration = this.spotlightScaleDuration * 0.3f;

        var seq = DOTween.Sequence();
        seq.Append(this.spotlightRect.DOScale(this.spotlightPunchScale, punchDuration).SetEase(Ease.OutQuad));
        seq.Append(this.spotlightRect.DOScale(1f, settleDuration).SetEase(Ease.InQuad));

        await seq.AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(ct)
            .SuppressCancellationThrow();
    }

    private async UniTaskVoid ShowStepDescription(int stepIndex, CancellationToken ct)
    {
        HideAllDescriptions();

        if (stepIndex < 0 || stepIndex >= this.stepDescriptions.Length) return;

        TMP_Text desc = this.stepDescriptions[stepIndex];
        desc.gameObject.SetActive(true);
        desc.color = new Color(desc.color.r, desc.color.g, desc.color.b, 0f);

        await desc.DOFade(1f, this.descriptionFadeDuration)
            .SetEase(Ease.OutCubic)
            .AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(ct)
            .SuppressCancellationThrow();
    }

    private void HideAllDescriptions()
    {
        foreach (TMP_Text desc in this.stepDescriptions)
        {
            if (desc == null) continue;
            DOTween.Kill(desc);
            desc.gameObject.SetActive(false);
        }
    }

    private async UniTask FadeTooltip(bool fadeIn, CancellationToken ct)
    {
        float target = fadeIn ? 1f : 0f;
        await this.tooltipCanvasGroup
            .DOFade(target, this.tooltipFadeDuration)
            .SetEase(Ease.OutCubic)
            .AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(ct)
            .SuppressCancellationThrow();

        if (fadeIn)
            StartNextButtonPulse();
        else
            StopNextButtonPulse();
    }

    private void StartNextButtonPulse()
    {
        RectTransform btnTextRect = this.nextButtonText.rectTransform;
        DOTween.Kill(btnTextRect);
        btnTextRect.localScale = Vector3.one;
        btnTextRect.DOScale(1.1f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    private void StopNextButtonPulse()
    {
        RectTransform btnTextRect = this.nextButtonText.rectTransform;
        DOTween.Kill(btnTextRect);
        btnTextRect.localScale = Vector3.one;
    }

    private void KillAllTweens()
    {
        DOTween.Kill(this.darkOverlayImage);
        DOTween.Kill(this.spotlightRect);
        DOTween.Kill(this.tooltipCanvasGroup);
        foreach (TMP_Text desc in this.stepDescriptions)
        {
            if (desc != null) DOTween.Kill(desc);
        }
        StopNextButtonPulse();
    }
}
