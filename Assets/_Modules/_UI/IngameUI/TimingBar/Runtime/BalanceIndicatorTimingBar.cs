using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class BalanceIndicatorTimingBar : TimingBar
{
    [Header("References")] [SerializeField]
    private RectTransform indicator;

    [SerializeField] private RectTransform runningArea;
    [SerializeField] private GameObject failedFX;
    [SerializeField] private GameObject trueFX;

    [Header("Settings")] [SerializeField] private float indicatorSpeed = 200f;
    [SerializeField] private float indicatorEffectDuration = 0.2f;

    private float minPosition;
    private float maxPosition;
    private float direction;

    private void Awake()
    {
        GetMinMaxIndicatorPosition();
    }

    private void GetMinMaxIndicatorPosition()
    {
        if (this.runningArea == null) return;

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

        float currentX = this.indicator.anchoredPosition.x;
        currentX += this.direction * this.indicatorSpeed * Time.deltaTime;

        if (currentX >= this.maxPosition)
        {
            currentX = this.maxPosition;
            // this.direction = -1f;
        }
        else if (currentX <= this.minPosition)
        {
            currentX = this.minPosition;
            // this.direction = 1f;
        }

        this.indicator.anchoredPosition = new Vector2(currentX, this.indicator.anchoredPosition.y);
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
        await UniTask.WaitForSeconds(0.4f);
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


    [Button]
    public override void TapTimingBar()
    {
        this.direction = -this.direction;
    }

    [Button]
    public void ConfirmTiming()
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

        this.indicator.anchoredPosition = new Vector2((this.minPosition + this.maxPosition) / 2, this.indicator.anchoredPosition.y);

        SetInitialDirection();
    }

    private void SetInitialDirection()
    {
        float currentX = this.indicator.anchoredPosition.x;
        float distToMin = Mathf.Abs(currentX - this.minPosition);
        float distToMax = Mathf.Abs(currentX - this.maxPosition);

        this.direction = distToMin <= distToMax ? -1f : 1f;

        if (Mathf.Approximately(distToMin, distToMax))
            this.direction = 1f;
    }

    public override void RandomTrueArea()
    {
        if (this.trueArea == null || this.runningArea == null) return;

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
        if (this.indicator == null || this.trueArea == null || !this.isRunning) return false;

        float indicatorX = this.indicator.anchoredPosition.x;
        float trueAreaX = this.trueArea.anchoredPosition.x;
        float trueAreaWidth = this.trueArea.rect.width;
        float trueAreaLeft = trueAreaX - (trueAreaWidth / 2f);
        float trueAreaRight = trueAreaX + (trueAreaWidth / 2f);

        return indicatorX >= trueAreaLeft && indicatorX <= trueAreaRight;
    }

    private async UniTask ScaleUIEffect(RectTransform uiItem, float targetValue, float duration, bool popEffect, float delay)
    {
        uiItem.localScale = Vector3.zero;
        await UniTask.WaitForSeconds(delay);
        if (popEffect)
            await uiItem.DOScale(targetValue + 0.2f, duration).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();

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