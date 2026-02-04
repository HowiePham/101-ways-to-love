using System;
using System.Collections.Generic;
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
    [SerializeField] private RectTransform stepTutorialUI;

    [Header("Button")] [SerializeField] private Button settingBtn;
    [SerializeField] private Button skipBtn;
    [SerializeField] private Button hintBtn;
    [SerializeField] private Button removeAdsButton;

    [Header("Popup Effect")] [SerializeField]
    private RectTransform[] showingEffectUIs;

    [SerializeField] private Vector3 skipShowingPos;
    [SerializeField] private Vector3 skipHidingPos;
    [SerializeField] private Vector3 hintShowingPos;
    [SerializeField] private Vector3 hintHidingPos;

    private TweenerCore<Vector2, Vector2, VectorOptions> tutorialStepUITween;
    private List<StepPoint> stepPoints;
    public Action OnSettingClicked;
    public Action OnSkipClicked;
    public Action OnHintClicked;
    public Action OnRemoveAdsClicked;

    public override void Initialize()
    {
        base.Initialize();
        this.wrongSignal.gameObject.SetActive(false);
        this.stepTutorialUI.gameObject.SetActive(false);
        this.wrongSignal.localScale = Vector3.zero;
        this.stepPoints = new List<StepPoint>();

        this.settingBtn.onClick.AddListener(() => this.OnSettingClicked?.Invoke());
        this.skipBtn.onClick.AddListener(() => this.OnSkipClicked?.Invoke());
        this.hintBtn.onClick.AddListener(() => this.OnHintClicked?.Invoke());
        this.removeAdsButton.onClick.AddListener(() => OnRemoveAdsClicked?.Invoke());
    }

    public override void Show()
    {
        base.Show();

        HandleUIEffect();
    }

    private async UniTask HandleUIEffect()
    {
        var scalingTask = new UniTask[this.showingEffectUIs.Length];
        for (var i = 0; i < this.showingEffectUIs.Length; i++)
        {
            RectTransform uiItem = this.showingEffectUIs[i];
            scalingTask[i] = ScaleUIEffect(uiItem, 0.5f);
        }

        await UniTask.WhenAll(scalingTask);

        LoopScalingUIEffect(this.removeAdsButton.GetComponent<RectTransform>(), 1.1f, 1f, 1f);
    }

    public void SetLevelCurrent(string level)
    {
        this.levelTextCurrent.text = "Level " + level;
    }

    public void SetActiveTutorialStepUI(bool value)
    {
        this.stepTutorialUI.gameObject.SetActive(value);

        if (value)
        {
            this.tutorialStepUITween = this.stepTutorialUI.DOAnchorPosY(this.stepTutorialUI.anchoredPosition.y - 15f, .75f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);
        }
        else if (this.tutorialStepUITween != null)
        {
            this.tutorialStepUITween.Kill();
            this.tutorialStepUITween = null;
        }
    }

    public void ShowWrongSignal()
    {
        this.wrongSignal.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(this.wrongSignal.DOScale(this.maxScale * Vector3.one, this.signalDuration / 2));
        sequence.Append(this.wrongSignal.DOScale(Vector3.zero, this.signalDuration / 2));
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
        this.hintBtn.gameObject.SetActive(value);

        if (!value)
        {
            return;
        }

        await MovingUIEffect(this.hintBtn.GetComponent<RectTransform>(), this.hintHidingPos, this.hintShowingPos, 1f, delay, true);
        LoopScalingUIEffect(this.hintBtn.GetComponent<RectTransform>(), 1.1f, 1f, 1f);
    }

    public async UniTask SetActiveSkipButton(bool value, float delay = 0f)
    {
        this.skipBtn.gameObject.SetActive(value);

        if (!value)
        {
            return;
        }

        await MovingUIEffect(this.skipBtn.GetComponent<RectTransform>(), this.skipHidingPos, this.skipShowingPos, 1f, delay, true);
        LoopScalingUIEffect(this.skipBtn.GetComponent<RectTransform>(), 1.1f, 1f, 1f);
    }

    private async UniTask MovingUIEffect(RectTransform uiItem, Vector3 firstPos, Vector3 targetPos, float duration, float delay, bool driftingEffect)
    {
        uiItem.anchoredPosition = firstPos;
        await UniTask.WaitForSeconds(delay);
        if (driftingEffect)
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

    private async UniTask ScaleUIEffect(RectTransform uiItem, float delay)
    {
        uiItem.localScale = Vector3.zero;
        await UniTask.WaitForSeconds(delay);
        await uiItem.DOScale(1.2f, 0.4f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        await uiItem.DOScale(1f, 0.2f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
    }

    private async UniTask LoopScalingUIEffect(RectTransform uiItem, float targetValue, float delay, float duration)
    {
        uiItem.localScale = Vector3.one;
        await UniTask.WaitForSeconds(delay);
        uiItem.DOScale(targetValue, duration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }
}