using System;
using System.Collections.Generic;
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

    private TweenerCore<Vector2, Vector2, VectorOptions> tutorialStepUITween;
    private List<StepPoint> stepPoints;
    public Action OnSettingClicked;
    public Action OnSkipClicked;
    public Action OnHintClicked;

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

        var count = 0;
        for (int i = 0; i < this.stepPoints.Count; i++)
        {
            StepPoint stepPoint = this.stepPoints[i];

            if (count < stepNumber)
            {
                stepPoint.gameObject.SetActive(true);
                stepPoint.SetDoneUI(false);
                stepPoint.SetStepText($"{count + 1}");
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
}