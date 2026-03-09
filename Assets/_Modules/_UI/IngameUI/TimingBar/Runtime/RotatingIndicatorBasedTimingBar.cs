using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class RotatingIndicatorBasedTimingBar : TimingBar
{
    [SerializeField] private RectTransform indicator;
    [SerializeField] private GameObject failedFX;
    [SerializeField] private GameObject trueFX;

    [Tooltip("X = min angle (e.g. -60), Y = max angle (e.g. 60) in degrees on Z axis")] [SerializeField]
    private Vector2 rotatingLimitation;

    [SerializeField] private float indicatorEffectDuration;
    [SerializeField] private Vector2 trueAngleLimitation;

    private float currentTime;
    private float trueAreaCenterAngle;

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

        float targetAngle = Mathf.Lerp(this.rotatingLimitation.x, this.rotatingLimitation.y, progress);

        this.indicator.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
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

        this.indicator.localRotation = Quaternion.Euler(0f, 0f, this.rotatingLimitation.x);
    }

    public override void RandomTrueArea()
    {
        if (this.trueArea == null) return;

        float minAngle = this.rotatingLimitation.x + this.trueAngleLimitation.x;
        float maxAngle = this.rotatingLimitation.y - this.trueAngleLimitation.y;

        this.trueAreaCenterAngle = Random.Range(minAngle, maxAngle);

        this.trueArea.localRotation = Quaternion.Euler(0f, 0f, this.trueAreaCenterAngle);
    }

    [Button]
    private void CheckTiming()
    {
        Debug.Log($"Is Timing True: {IsTrueTiming()}");
    }

    public override bool IsTrueTiming()
    {
        if (this.indicator == null || this.trueArea == null) return false;

        float indicatorAngle = NormalizeAngle(this.indicator.eulerAngles.z);
        
        return indicatorAngle <= this.trueAngleLimitation.y && indicatorAngle >= this.trueAngleLimitation.x;
    }

    public override void DecreaseTrueAreaWidth(float value)
    {
        
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
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