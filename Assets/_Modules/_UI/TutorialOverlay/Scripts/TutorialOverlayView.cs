using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.UI;

public class TutorialOverlayView : BaseView
{
    [Header("Overlay")]
    [SerializeField] private CanvasGroup overlayCanvasGroup;

    [Header("Slide Card")]
    [SerializeField] private CanvasGroup slideCardCanvasGroup;

    [Header("Navigation")]
    [SerializeField] private Button tapToContinueButton;

    [Header("Container")]
    [SerializeField] private GameObject tutorialRoot;

    [Header("Steps")]
    [SerializeField] private TutorialStepData[] steps;

    [Header("Animation Settings")]
    [SerializeField] private float overlayFadeDuration = 0.35f;
    [SerializeField] private float overlayTargetAlpha = 0.95f;
    [SerializeField] private float cardFadeDuration = 0.25f;

    public Action OnNextClicked;

    public TutorialStepData[] Steps => this.steps;

    private GameObject currentStepContent;

    public override void Initialize()
    {
        base.Initialize();
        this.overlayCanvasGroup.alpha = 0f;
        this.slideCardCanvasGroup.alpha = 0f;
        HideAllStepContents();
        this.tapToContinueButton.onClick.AddListener(() => OnNextClicked?.Invoke());
    }

    public override void Show()
    {
        // Do NOT call base.Show() — it disables the shared gameplay Canvas
        this.tutorialRoot.SetActive(true);
        this.overlayCanvasGroup.alpha = 0f;
        this.slideCardCanvasGroup.alpha = 0f;
        HideAllStepContents();
    }

    public override void Hide()
    {
        KillAllTweens();
        this.overlayCanvasGroup.alpha = 0f;
        this.slideCardCanvasGroup.alpha = 0f;
        HideAllStepContents();
        // Do NOT call base.Hide() — it disables the shared gameplay Canvas
        this.tutorialRoot.SetActive(false);
    }

    public async UniTask PlayIntroAnimation(CancellationToken ct)
    {
        await this.overlayCanvasGroup
            .DOFade(this.overlayTargetAlpha, this.overlayFadeDuration)
            .SetEase(Ease.OutCubic)
            .AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(ct)
            .SuppressCancellationThrow();
    }

    public async UniTask TransitionToStep(TutorialStepData stepData, CancellationToken ct)
    {
        // Fade out card if already visible (skip on first step)
        if (this.slideCardCanvasGroup.alpha > 0f)
        {
            await this.slideCardCanvasGroup
                .DOFade(0f, this.cardFadeDuration)
                .SetEase(Ease.InCubic)
                .AsyncWaitForCompletion()
                .AsUniTask()
                .AttachExternalCancellation(ct)
                .SuppressCancellationThrow();

            if (ct.IsCancellationRequested) return;
        }

        // Swap active step content
        if (this.currentStepContent != null)
            this.currentStepContent.SetActive(false);

        this.currentStepContent = stepData.stepContent;

        if (this.currentStepContent != null)
            this.currentStepContent.SetActive(true);

        // Fade in card
        await this.slideCardCanvasGroup
            .DOFade(1f, this.cardFadeDuration)
            .SetEase(Ease.OutCubic)
            .AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(ct)
            .SuppressCancellationThrow();
    }

    public async UniTask PlayOutroAnimation(CancellationToken ct)
    {
        KillAllTweens();
        var seq = DOTween.Sequence();
        seq.Join(this.slideCardCanvasGroup.DOFade(0f, this.cardFadeDuration).SetEase(Ease.InCubic));
        seq.Join(this.overlayCanvasGroup.DOFade(0f, this.overlayFadeDuration).SetEase(Ease.InCubic));

        await seq.AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(ct)
            .SuppressCancellationThrow();
    }

    private void HideAllStepContents()
    {
        this.currentStepContent = null;
        foreach (TutorialStepData step in this.steps)
        {
            if (step.stepContent != null)
                step.stepContent.SetActive(false);
        }
    }

    private void KillAllTweens()
    {
        DOTween.Kill(this.overlayCanvasGroup);
        DOTween.Kill(this.slideCardCanvasGroup);
    }
}
