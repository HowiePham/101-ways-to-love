using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class IndicatorBasedTimingBar : TimingBar
{
    [SerializeField] private RectTransform indicator;
    [SerializeField] private RectTransform runningArea;
    [SerializeField] private GameObject failedFX;
    [SerializeField] private GameObject trueFX;
    [SerializeField] private float indicatorEffectDuration;
    private float minPosition;
    private float maxPosition;
    private float currentTime;

    private void Awake()
    {
        GetMinMaxIndicatorPosition();
    }

    private void GetMinMaxIndicatorPosition()
    {
        if (this.runningArea == null)
        {
            return;
        }

        float barWidth = this.runningArea.rect.width;
        this.minPosition = -barWidth / 2f;
        this.maxPosition = barWidth / 2f;
    }

    private void Update()
    {
        UpdateIndicatorMovement();
    }

    private void UpdateIndicatorMovement()
    {
        if (!this.isRunning || this.indicator == null) return;

        this.currentTime += Time.deltaTime;

        if (this.currentTime >= this.cycleTime * 2)
        {
            this.currentTime -= this.cycleTime * 2;
        }

        float progress;

        if (this.currentTime < this.cycleTime)
        {
            progress = this.currentTime / this.cycleTime;
        }
        else
        {
            progress = 1f - (this.currentTime - this.cycleTime) / this.cycleTime;
        }

        float indicatorX = Mathf.Lerp(this.minPosition, this.maxPosition, progress);

        this.indicator.anchoredPosition = new Vector2(indicatorX, this.indicator.anchoredPosition.y);
    }

    public override async UniTask Show()
    {
        this.timingBarContainer.localScale = Vector3.zero;
        this.trueArea.localScale = Vector3.zero;
        this.indicator.localScale = Vector3.zero;
        ResetBar();

        ScaleUIEffect(this.timingBarContainer, 1f, 0.4f, true, 0f);
        await ScaleUIEffect(this.trueArea, 1f, 0.4f, false, 0f);

        await ScaleUIEffect(this.indicator, 1f, 0.4f, true, 0f);
    }

    public override async UniTask Hide()
    {
        ScaleUIEffect(this.timingBarContainer, 0f, 0.4f, false, 0f);
        ScaleUIEffect(this.trueArea, 0f, 0.4f, false, 0f);
        ScaleUIEffect(this.indicator, 0f, 0.4f, false, 0f);
    }

    public override void StartRunning()
    {
        ResetBar();
        this.isRunning = true;
    }

    public override void ResumeRunning()
    {
        this.isRunning = true;
    }

    public override void StopRunning()
    {
        this.isRunning = false;
    }

    public override void TapTimingBar()
    {
        StopRunning();
        ScaleUIEffect(this.indicator, 1.2f, this.indicatorEffectDuration, true);

        if (IsTrueTiming())
        {
            this.failedFX.SetActive(false);
            this.trueFX.SetActive(true);
        }
        else
        {
            this.trueFX.SetActive(false);
            this.failedFX.SetActive(true);
        }
    }

    public override void ResetBar()
    {
        this.failedFX.SetActive(false);
        this.trueFX.SetActive(false);

        this.currentTime = 0f;
        this.indicator.anchoredPosition = new Vector2(this.minPosition, this.indicator.anchoredPosition.y);
    }

    public override void RandomTrueArea()
    {
        if (this.trueArea == null || this.runningArea == null)
        {
            return;
        }

        float trueAreaWidth = this.trueArea.rect.width;

        float minRandomX = this.minPosition + (trueAreaWidth / 2f);
        float maxRandomX = this.maxPosition - (trueAreaWidth / 2f);

        float randomX = Random.Range(minRandomX, maxRandomX);
        this.trueArea.anchoredPosition = new Vector2(randomX, this.trueArea.anchoredPosition.y);
    }

    [Button]
    private void CheckTiming()
    {
        Debug.Log($"Is Timing True: {IsTrueTiming()}");
    }

    public override bool IsTrueTiming()
    {
        if (this.indicator == null || this.trueArea == null)
        {
            return false;
        }

        float indicatorX = this.indicator.anchoredPosition.x;

        float trueAreaX = this.trueArea.anchoredPosition.x;
        float trueAreaWidth = this.trueArea.rect.width;

        float trueAreaLeft = trueAreaX - (trueAreaWidth / 2f);
        float trueAreaRight = trueAreaX + (trueAreaWidth / 2f);

        return indicatorX >= trueAreaLeft && indicatorX <= trueAreaRight;
    }

    public override void DecreaseTrueAreaWidth(float value)
    {
        Vector2 currentSize = this.trueArea.sizeDelta;
        float newWidth = this.trueArea.sizeDelta.x - value;
        this.trueArea.sizeDelta = new Vector2(newWidth, currentSize.y);
    }

    private async UniTask ScaleUIEffect(RectTransform uiItem, float targetValue, float duration, bool popEffect, float delay)
    {
        uiItem.localScale = Vector3.zero;
        await UniTask.WaitForSeconds(delay);
        if (popEffect)
        {
            await uiItem.DOScale(targetValue + 0.2f, duration).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        }

        await uiItem.DOScale(targetValue, duration).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
    }

    private async UniTask ScaleUIEffect(RectTransform uiItem, float targetValue, float duration, bool scaleBack = false)
    {
        Vector3 originalScale = uiItem.localScale;
        if (scaleBack)
        {
            await uiItem.DOScale(targetValue, duration / 2).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
            await uiItem.DOScale(originalScale, duration / 2).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
            return;
        }

        await uiItem.DOScale(targetValue, duration).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
    }
}