using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Games;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.UI;

public class SettingView : BaseView
{
    [SerializeField] private RectTransform settingPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Toggle musicButton;
    [SerializeField] private Toggle soundButton;
    [SerializeField] private Toggle vibrationButton;

    public Action OnCloseClicked;
    public Action<bool> OnMusicClicked;
    public Action<bool> OnSoundClicked;
    public Action<bool> OnVibrationClicked;

    private Sequence settingPanelEffectSequence;

    public override void Initialize()
    {
        base.Initialize();

        this.closeButton.onClick.AddListener(() => this.OnCloseClicked?.Invoke());
        this.musicButton.onValueChanged.AddListener((value) => this.OnMusicClicked?.Invoke(value));
        this.soundButton.onValueChanged.AddListener((value) => this.OnSoundClicked?.Invoke(value));
        this.vibrationButton.onValueChanged.AddListener((value) => this.OnVibrationClicked?.Invoke(value));

        this.settingPanelEffectSequence = DOTween.Sequence();
        this.settingPanel.localScale = Vector3.zero;
    }

    public override void Show()
    {
        base.Show();

        RunSettingPanelEffectSequence();
    }

    private async UniTask RunSettingPanelEffectSequence()
    {
        await this.settingPanelEffectSequence.Append(this.settingPanel.DOScale(1f, 0.4f)).AsyncWaitForCompletion();
    }

    public override void Hide()
    {
        base.Hide();

        this.settingPanelEffectSequence.Kill();
        this.settingPanel.localScale = Vector3.zero;
    }

    public void UpdateToggleValue(SettingModel settingModel)
    {
        this.musicButton.isOn = settingModel.MusicOn;
        this.soundButton.isOn = settingModel.SoundOn;
        this.vibrationButton.isOn = settingModel.VibrationOn;

        UpdateToggleSprite(this.musicButton);
        UpdateToggleSprite(this.soundButton);
        UpdateToggleSprite(this.vibrationButton);
    }

    private void UpdateToggleSprite(Toggle toggle)
    {
        var toggleSpriteSwap = toggle.GetComponent<SpriteSwapToggle>();
        toggleSpriteSwap.ValueChangeHandler(toggle.isOn);
    }
}