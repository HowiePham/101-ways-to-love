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
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("Steps")]
    [SerializeField] private TutorialStepData[] steps;

    [Header("Animation Settings")]
    [SerializeField] private float overlayFadeDuration = 0.35f;
    [SerializeField] private float overlayTargetAlpha = 0.75f;
    [SerializeField] private float tooltipFadeDuration = 0.25f;
    [SerializeField] private float spotlightPunchScale = 1.15f;
    [SerializeField] private float spotlightScaleDuration = 0.3f;
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
        base.Show();
        // Reset state in case this is reshown
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
        base.Hide();
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

        // 3. Position tooltip near target (or center if no target)
        if (targetRect != null)
            PositionTooltipNearTarget(targetRect);
        else
            CenterTooltip();

        // 4. Populate text
        this.titleText.text = stepData.title;
        this.descriptionText.text = stepData.description;
        this.nextButtonText.text = stepData.isLastStep ? this.nextButtonLabelFinish : this.nextButtonLabelNext;

        // 5. Fade in tooltip
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

    private void PositionTooltipNearTarget(RectTransform target)
    {
        Canvas rootCanvas = GetComponent<Canvas>();
        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
        Camera uiCamera = rootCanvas.worldCamera;

        Vector3 targetWorldCenter = target.TransformPoint(target.rect.center);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, targetWorldCenter);

        float viewportY = screenPoint.y / Screen.height;
        bool placeAbove = viewportY < 0.5f;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out Vector2 localPos);

        float targetHalfHeight = target.rect.height * 0.5f * target.lossyScale.y / canvasRect.lossyScale.y;
        float tooltipHalfHeight = this.tooltipPanel.rect.height * 0.5f;
        float gap = 24f;

        float yOffset = placeAbove
            ? localPos.y + targetHalfHeight + tooltipHalfHeight + gap
            : localPos.y - targetHalfHeight - tooltipHalfHeight - gap;

        float canvasHalfWidth = canvasRect.rect.width * 0.5f;
        float tooltipHalfWidth = this.tooltipPanel.rect.width * 0.5f;
        float clampedX = Mathf.Clamp(localPos.x,
            -canvasHalfWidth + tooltipHalfWidth + 16f,
             canvasHalfWidth - tooltipHalfWidth - 16f);

        this.tooltipPanel.anchoredPosition = new Vector2(clampedX, yOffset);
    }

    private void CenterTooltip()
    {
        this.tooltipPanel.anchoredPosition = Vector2.zero;
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
    }

    private void KillAllTweens()
    {
        DOTween.Kill(this.darkOverlayImage);
        DOTween.Kill(this.spotlightRect);
        DOTween.Kill(this.tooltipCanvasGroup);
    }
}
