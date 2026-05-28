using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : BaseView
{
    [SerializeField] private Transform wrongSignal;
    [SerializeField] private float signalDuration;
    [SerializeField] private float maxScale;
    [Header("Text")] [SerializeField] private TMP_Text levelTextCurrent;

    [Header("StepUI")] [SerializeField] private Transform stepPanel;
    [SerializeField] private StepPoint stepPointPrefab;

    [Header("Life View")] [SerializeField] private NumberBasedLifeView lifeView;
    [SerializeField] private RectTransform lifePanel;

    [Header("Button")] [SerializeField] private Button settingBtn;
    [SerializeField] private Button skipBtn;
    [SerializeField] private Button hintBtn;
    [SerializeField] private Button removeAdsButton;
    [SerializeField] private Button startLevelGameButton;
    [SerializeField] private Button noLifeBlocker;

    [Header("Popup Effect")] [SerializeField]
    private RectTransform[] showingEffectUIs;

    private TweenerCore<Vector2, Vector2, VectorOptions> tutorialStepUITween;
    private Dictionary<RectTransform, TweenerCore<Vector3, Vector3, VectorOptions>> loopScalingTweens;
    private Dictionary<RectTransform, TweenerCore<Vector3, Vector3, VectorOptions>> movingTweens;
    private List<StepPoint> stepPoints;
    private CancellationTokenSource showCts;
    private RectTransform RemoveAdsRect => this.removeAdsButton.GetComponent<RectTransform>();
    private RectTransform HintBtnRect => this.hintBtn.GetComponent<RectTransform>();
    private RectTransform SkipBtnRect => this.skipBtn.GetComponent<RectTransform>();
    private RectTransform StartLevelGameBtnRect => this.startLevelGameButton.GetComponent<RectTransform>();
    public NumberBasedLifeView LifeView => this.lifeView;

    public Action OnSettingClicked;
    public Action OnSkipClicked;
    public Action OnHintClicked;
    public Action OnRemoveAdsClicked;
    public Action OnStartLevelGameClicked;
    public Action OnNoLifeBlockerClicked;

    public override void Initialize()
    {
        base.Initialize();
        if (this.lifeView != null)
        {
            this.lifeView.Initialize();
        }

        this.wrongSignal.gameObject.SetActive(false);
        this.wrongSignal.localScale = Vector3.zero;
        this.stepPoints = new List<StepPoint>();
        this.loopScalingTweens = new Dictionary<RectTransform, TweenerCore<Vector3, Vector3, VectorOptions>>();

        this.settingBtn.onClick.AddListener(() => this.OnSettingClicked?.Invoke());
        this.skipBtn.onClick.AddListener(() => this.OnSkipClicked?.Invoke());
        this.hintBtn.onClick.AddListener(() => this.OnHintClicked?.Invoke());
        this.removeAdsButton.onClick.AddListener(() => OnRemoveAdsClicked?.Invoke());
        this.startLevelGameButton.onClick.AddListener(() => OnStartLevelGameClicked?.Invoke());
        this.noLifeBlocker.onClick.AddListener(() => OnNoLifeBlockerClicked?.Invoke());
        this.noLifeBlocker.gameObject.SetActive(false);
    }

    public override void Show()
    {
        base.Show();

        this.showCts?.Cancel();
        this.showCts?.Dispose();
        this.showCts = new CancellationTokenSource();

        HandleUIEffect(this.showCts.Token);
    }

    public override void Hide()
    {
        base.Hide();

        this.showCts?.Cancel();
        this.showCts?.Dispose();
        this.showCts = null;

        if (this.loopScalingTweens != null)
        {
            foreach (var kvp in this.loopScalingTweens)
            {
                kvp.Value?.Kill();
            }
            this.loopScalingTweens.Clear();
        }

        DOTween.Kill(this.HintBtnRect);
        DOTween.Kill(this.SkipBtnRect);
        DOTween.Kill(this.RemoveAdsRect);
        DOTween.Kill(this.StartLevelGameBtnRect);

        this.hintBtn.gameObject.SetActive(false);
        this.skipBtn.gameObject.SetActive(false);
        this.startLevelGameButton.gameObject.SetActive(false);

        this.HintBtnRect.localScale = Vector3.one;
        this.SkipBtnRect.localScale = Vector3.one;
        this.StartLevelGameBtnRect.localScale = Vector3.one;
    }

    private async UniTask HandleUIEffect(CancellationToken ct)
    {
        this.HintBtnRect.localScale = Vector3.zero;
        this.SkipBtnRect.localScale = Vector3.zero;

        var scalingTask = new UniTask[this.showingEffectUIs.Length];
        for (var i = 0; i < this.showingEffectUIs.Length; i++)
        {
            RectTransform uiItem = this.showingEffectUIs[i];
            scalingTask[i] = ScaleUIEffect(uiItem, 0.5f, ct);
        }

        if (ct.IsCancellationRequested) return;
        await UniTask.WhenAll(scalingTask);
        if (ct.IsCancellationRequested) return;

        // LoopScalingUIEffect(this.RemoveAdsRect, 1.1f, 1f, 1f);
    }

    public void SetLevelCurrent(string level)
    {
        this.levelTextCurrent.text = "Level " + level;
    }

    public void SetActiveTutorialStepUI(bool value)
    {
    }

    public async UniTask ShowWrongSignal()
    {
        this.wrongSignal.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(this.wrongSignal.DOScale(this.maxScale * Vector3.one, this.signalDuration / 2));
        sequence.Append(this.wrongSignal.DOScale(Vector3.zero, this.signalDuration / 2));

        await sequence.AsyncWaitForCompletion();
    }

    public void InitStepPoint(int stepNumber)
    {
        if (this.stepPoints.Count < stepNumber)
        {
            var diff = stepNumber - this.stepPoints.Count;

            for (int i = 0; i < diff; i++)
            {
                StepPoint stepPoint = Instantiate(this.stepPointPrefab, this.stepPanel);
                this.stepPoints.Add(stepPoint);
            }
        }

        var count = 1;
        for (int i = 0; i < this.stepPoints.Count; i++)
        {
            StepPoint stepPoint = this.stepPoints[i];

            if (count <= stepNumber)
            {
                stepPoint.gameObject.SetActive(true);
                stepPoint.SetDoneUI(false);
                stepPoint.SetStepText($"{count}");
                stepPoint.SetActiveBridgeImage(count < stepNumber);
                count++;
                continue;
            }

            stepPoint.gameObject.SetActive(false);
        }
    }

    public void UpdateStepPoint()
    {
        foreach (StepPoint stepPoint in this.stepPoints)
        {
            if (stepPoint.IsChecked)
            {
                continue;
            }

            stepPoint.SetDoneUI(true);
            break;
        }
    }

    public async UniTask SetActiveHintButton(bool value, float delay = 0f)
    {
        if (!value)
        {
            this.hintBtn.gameObject.SetActive(false);
            KillLoopScalingTween(this.HintBtnRect);
            this.HintBtnRect.localScale = Vector3.one;
            return;
        }

        if (this.showCts == null) return;

        this.hintBtn.gameObject.SetActive(true);
        var ct = this.showCts.Token;
        await ScaleUIEffect(this.HintBtnRect, delay, ct);
        if (ct.IsCancellationRequested) return;
        LoopScalingUIEffect(this.HintBtnRect, 1.1f, 1f, 1f, ct).Forget();
    }

    public async UniTask SetActiveStartLevelGameButton(bool value, float delay = 0f)
    {
        if (!value)
        {
            this.startLevelGameButton.gameObject.SetActive(false);
            KillLoopScalingTween(this.StartLevelGameBtnRect);
            this.StartLevelGameBtnRect.localScale = Vector3.one;
            return;
        }

        if (this.showCts == null) return;

        this.startLevelGameButton.gameObject.SetActive(true);
        var ct = this.showCts.Token;
        await ScaleUIEffect(this.StartLevelGameBtnRect, delay, ct);
        if (ct.IsCancellationRequested) return;
        LoopScalingUIEffect(this.StartLevelGameBtnRect, 1.1f, 1f, 1f, ct).Forget();
    }

    public async UniTask SetActiveSkipButton(bool value, float delay = 0f)
    {
        if (!value)
        {
            this.skipBtn.gameObject.SetActive(false);
            KillLoopScalingTween(this.SkipBtnRect);
            this.SkipBtnRect.localScale = Vector3.one;
            return;
        }

        if (this.showCts == null) return;

        this.skipBtn.gameObject.SetActive(true);
        var ct = this.showCts.Token;
        await ScaleUIEffect(this.SkipBtnRect, delay, ct);
        if (ct.IsCancellationRequested) return;
        LoopScalingUIEffect(this.SkipBtnRect, 1.1f, 1f, 1f, ct).Forget();
    }

    private void KillLoopScalingTween(RectTransform uiItem)
    {
        if (this.loopScalingTweens != null && this.loopScalingTweens.TryGetValue(uiItem, out var tween))
        {
            tween?.Kill();
            this.loopScalingTweens.Remove(uiItem);
        }
    }

    private async UniTask ScaleDownAndHideButton(RectTransform rect, Button btn)
    {
        if (!btn.gameObject.activeSelf) return;
        KillLoopScalingTween(rect);
        await rect.DOScale(0f, 0.3f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        btn.gameObject.SetActive(false);
        rect.localScale = Vector3.one;
    }

    public void HideButtonsOnLevelDone()
    {
        ScaleDownAndHideButton(this.HintBtnRect, this.hintBtn).Forget();
        ScaleDownAndHideButton(this.SkipBtnRect, this.skipBtn).Forget();
    }

    private async UniTask MovingUIEffect(RectTransform uiItem, Vector3 firstPos, Vector3 targetPos, float duration, float delay, bool bounceEffect)
    {
        uiItem.anchoredPosition = firstPos;
        await UniTask.WaitForSeconds(delay);
        if (bounceEffect)
        {
            var driftingPos = new Vector3(targetPos.x - 10f, targetPos.y, targetPos.z);
            await uiItem.DOAnchorPos(driftingPos, duration * 2 / 3).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
            await uiItem.DOAnchorPos(targetPos, duration / 3).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        }
        else
        {
            await uiItem.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        }
    }

    private async UniTask ScaleUIEffect(RectTransform uiItem, float delay, CancellationToken ct = default)
    {
        uiItem.localScale = Vector3.zero;
        bool canceled = await UniTask.WaitForSeconds(delay, cancellationToken: ct).SuppressCancellationThrow();
        if (canceled) return;
        await uiItem.DOScale(1.2f, 0.4f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;
        await uiItem.DOScale(1f, 0.2f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
    }

    public void SetActiveNoLifeBlocker(bool active)
    {
        this.noLifeBlocker.gameObject.SetActive(active);
    }

    private async UniTask LoopScalingUIEffect(RectTransform uiItem, float targetValue, float delay, float duration, CancellationToken ct = default)
    {
        uiItem.localScale = Vector3.one;
        bool canceled = await UniTask.WaitForSeconds(delay, cancellationToken: ct).SuppressCancellationThrow();
        if (canceled) return;

        if (this.loopScalingTweens.TryGetValue(uiItem, out var existing))
        {
            existing?.Kill();
        }
        TweenerCore<Vector3, Vector3, VectorOptions> tweenCore = uiItem.DOScale(targetValue, duration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        this.loopScalingTweens[uiItem] = tweenCore;
    }
}