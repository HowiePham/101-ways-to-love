using System;
using DG.Tweening;
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

    [Header("Button")] [SerializeField] private Button settingBtn;
    [SerializeField] private Button skipBtn;

    public Action OnSettingClicked;
    public Action OnSkipClicked;

    public override void Initialize()
    {
        base.Initialize();
        this.wrongSignal.gameObject.SetActive(false);
        this.wrongSignal.localScale = Vector3.zero;

        this.settingBtn.onClick.AddListener(() => this.OnSettingClicked?.Invoke());
        this.skipBtn.onClick.AddListener(() => this.OnSkipClicked?.Invoke());
    }

    public void SetLevelCurrent(string level)
    {
        this.levelTextCurrent.text = "Level " + level;
    }

    public void ShowWrongSignal()
    {
        this.wrongSignal.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(this.wrongSignal.DOScale(this.maxScale * Vector3.one, this.signalDuration / 2));
        sequence.Append(this.wrongSignal.DOScale(Vector3.zero, this.signalDuration / 2));
    }
}