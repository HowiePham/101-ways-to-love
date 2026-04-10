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
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;

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
    [SerializeField] private float descriptionOffset = 24f;
    [SerializeField] private float screenEdgeMargin = 16f;
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

        this.nextButton.onClick.AddListener(() => OnNextClicked?.Invoke());
    }

    public override void Show()
    {
        // Do NOT call base.Show() — it disables the shared gameplay Canvas
        this.tutorialRoot.SetActive(true);
        this.darkOverlayImage.color = new Color(0f, 0f, 0f, 0f);
        this.tooltipCanvasGroup.alpha = 0f;
        this.spotlightRect.localScale = Vector3.zero;
    }

    public override void Hide()
    {
        KillAllTweens();
        this.darkOverlayImage.color = new Color(0f, 0f, 0f, 0f);
        this.tooltipCanvasGroup.alpha = 0f;
        this.spotlightRect.localScale = Vector3.zero;
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

    public async UniTask TransitionToStep(TutorialStepData stepData, RectTransform targetRect, CancellationToken ct)
    {
        // 1. Fade out tooltip
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

        PositionDescriptionBelowSpotlight();

        this.descriptionText.text = stepData.description;
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
        // Position and size the spotlight to match the target
        this.unmask.FitTo(target);
        // Apply padding on top of the fitted size
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

    private void PositionDescriptionBelowSpotlight()
    {
        Canvas rootCanvas = GetComponent<Canvas>();
        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
        Camera uiCamera = rootCanvas.worldCamera;

        // Convert spotlight world center → canvas-local point
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, this.spotlightRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPoint, uiCamera, out Vector2 localPos);

        // Y: place below spotlight bottom edge (convert world-scale height to canvas units)
        float spotlightHalfHeight = this.spotlightRect.rect.height * 0.5f
            * this.spotlightRect.lossyScale.y / canvasRect.lossyScale.y;
        float targetY = localPos.y - spotlightHalfHeight - this.descriptionOffset;

        // X: center on spotlight, then clamp so text never clips screen edges
        float canvasHalfWidth = canvasRect.rect.width * 0.5f;
        float textHalfWidth = this.descriptionText.rectTransform.rect.width * 0.5f;
        float clampedX = Mathf.Clamp(
            localPos.x,
            -canvasHalfWidth + textHalfWidth + this.screenEdgeMargin,
             canvasHalfWidth - textHalfWidth - this.screenEdgeMargin);

        this.descriptionText.rectTransform.anchoredPosition = new Vector2(clampedX, targetY);
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
        StopNextButtonPulse();
    }
}
