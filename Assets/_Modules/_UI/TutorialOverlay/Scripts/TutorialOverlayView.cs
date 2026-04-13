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

    [Header("Navigation")]
    [SerializeField] private Button tapToContinueButton;
    [SerializeField] private CanvasGroup nextButtonCanvasGroup;

    [Header("Container")]
    [SerializeField] private GameObject tutorialRoot;

    [Header("Steps")]
    [SerializeField] private TutorialStepData[] steps;

    [Header("Animation Settings")]
    [SerializeField] private float overlayFadeDuration = 0.35f;
    [SerializeField] private float overlayTargetAlpha = 0.95f;
    [SerializeField] private float cardScaleDuration = 0.25f;

    public Action OnNextClicked;

    public TutorialStepData[] Steps => this.steps;

    private GameObject currentStepContent;

    public override void Initialize()
    {
        base.Initialize();
        this.overlayCanvasGroup.alpha = 0f;
        this.nextButtonCanvasGroup.alpha = 0f;
        HideAllStepContents();
        this.tapToContinueButton.onClick.AddListener(() => OnNextClicked?.Invoke());
    }

    public override void Show()
    {
        // Do NOT call base.Show() — it disables the shared gameplay Canvas
        this.tutorialRoot.SetActive(true);
        this.overlayCanvasGroup.alpha = 0f;
        this.nextButtonCanvasGroup.alpha = 0f;
        HideAllStepContents();
    }

    public override void Hide()
    {
        KillAllTweens();
        this.overlayCanvasGroup.alpha = 0f;
        this.nextButtonCanvasGroup.alpha = 0f;
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
        // Fade out button + scale down current step simultaneously
        if (this.currentStepContent != null)
        {
            var outSeq = DOTween.Sequence();
            outSeq.Join(this.nextButtonCanvasGroup.DOFade(0f, this.cardScaleDuration).SetEase(Ease.InCubic));
            outSeq.Join(this.currentStepContent.transform.DOScale(0f, this.cardScaleDuration).SetEase(Ease.InBack));

            await outSeq.AsyncWaitForCompletion()
                .AsUniTask()
                .AttachExternalCancellation(ct)
                .SuppressCancellationThrow();

            if (ct.IsCancellationRequested) return;

            this.currentStepContent.SetActive(false);
        }

        // Activate and scale up the next step
        this.currentStepContent = stepData.stepContent;

        if (this.currentStepContent != null)
        {
            this.currentStepContent.transform.localScale = Vector3.zero;
            this.currentStepContent.SetActive(true);

            await this.currentStepContent.transform
                .DOScale(1f, this.cardScaleDuration)
                .SetEase(Ease.OutBack)
                .AsyncWaitForCompletion()
                .AsUniTask()
                .AttachExternalCancellation(ct)
                .SuppressCancellationThrow();

            if (ct.IsCancellationRequested) return;
        }

        // Fade in button after new step is fully visible
        await this.nextButtonCanvasGroup
            .DOFade(1f, this.cardScaleDuration)
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

        if (this.currentStepContent != null)
            seq.Join(this.currentStepContent.transform.DOScale(0f, this.cardScaleDuration).SetEase(Ease.InBack));

        seq.Join(this.nextButtonCanvasGroup.DOFade(0f, this.cardScaleDuration).SetEase(Ease.InCubic));
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
            if (step.stepContent == null) continue;
            step.stepContent.transform.localScale = Vector3.zero;
            step.stepContent.SetActive(false);
        }
    }

    private void KillAllTweens()
    {
        DOTween.Kill(this.overlayCanvasGroup);
        DOTween.Kill(this.nextButtonCanvasGroup);
        if (this.currentStepContent != null)
            DOTween.Kill(this.currentStepContent.transform);
    }
}
