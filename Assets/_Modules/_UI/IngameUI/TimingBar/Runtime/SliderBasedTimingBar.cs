using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class SliderBasedTimingBar : TimingBar
{
    [SerializeField] private Image runningBarImage;
    [SerializeField] private GameObject failedFX;
    [SerializeField] private GameObject trueFX;
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;
    [SerializeField] private float feedbackEffectDuration;
    private float currentTime;

    private void Update()
    {
        UpdateIndicatorMovement();
    }

    private void UpdateIndicatorMovement()
    {
        if (!this.isRunning || this.runningBarImage == null) return;

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

        float newValue = Mathf.Lerp(0, 1, progress);

        this.runningBarImage.fillAmount = newValue;
    }

    public override async UniTask Show()
    {
        this.timingBarContainer.localScale = Vector3.zero;
        this.trueArea.localScale = Vector3.zero;
        ResetBar();

        ScaleUIEffect(this.timingBarContainer, 1f, 0.4f, true, 0f);
        await ScaleUIEffect(this.trueArea, 1f, 0.4f, false, 0f);
    }

    public override async UniTask Hide()
    {
        ScaleUIEffect(this.timingBarContainer, 0f, 0.4f, false, 0f);
        ScaleUIEffect(this.trueArea, 0f, 0.4f, false, 0f);
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
        ScaleUIEffect(this.timingBarContainer, 1.2f, this.feedbackEffectDuration, true);

        if (IsTrueTiming())
        {
            // this.failedFX.SetActive(false);
            // this.trueFX.SetActive(true);
        }
        else
        {
            // this.trueFX.SetActive(false);
            // this.failedFX.SetActive(true);
        }
    }

    public override void ResetBar()
    {
        // this.failedFX.SetActive(false);
        // this.trueFX.SetActive(false);

        this.currentTime = 0f;
        this.runningBarImage.fillAmount = 0;
    }

    public override void RandomTrueArea()
    {
    }

    [Button]
    private void CheckTiming()
    {
        Debug.Log($"Is Timing True: {IsTrueTiming()}");
    }

    public override bool IsTrueTiming()
    {
        if (this.runningBarImage == null || this.trueArea == null)
        {
            return false;
        }

        float curVal = this.runningBarImage.fillAmount;

        return curVal >= this.minValue && curVal <= this.maxValue;
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