using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Games;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingView : BaseView
{
    [SerializeField] private RectTransform settingPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private CanvasGroup replayBtnGroup;
    [SerializeField] private CanvasGroup homeBtnGroup;
    [SerializeField] private Toggle musicButton;
    [SerializeField] private Toggle soundButton;
    [SerializeField] private Toggle vibrationButton;
    [SerializeField] private TMP_Text buildInfoText;

    public Action OnCloseClicked;
    public Action OnReplayClicked;
    public Action OnHomeClicked;
    public Action<bool> OnMusicClicked;
    public Action<bool> OnSoundClicked;
    public Action<bool> OnVibrationClicked;

    private Sequence settingPanelEffectSequence;

    public override void Initialize()
    {
        base.Initialize();

        this.closeButton.onClick.AddListener(() => this.OnCloseClicked?.Invoke());
        this.replayButton.onClick.AddListener(() => this.OnReplayClicked?.Invoke());
        this.homeButton.onClick.AddListener(() => this.OnHomeClicked?.Invoke());
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
        ShowCurrentBuildInfo();
    }

    private async UniTask RunSettingPanelEffectSequence()
    {
        this.replayBtnGroup.DOFade(0f, 0f);
        this.homeBtnGroup.DOFade(0f, 0f);

        await this.settingPanelEffectSequence.Append(this.settingPanel.DOScale(1f, 0.4f)).AsyncWaitForCompletion();

        this.replayBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
        await this.homeBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
    }

    public override void Hide()
    {
        base.Hide();

        this.settingPanelEffectSequence.Kill();
        this.settingPanel.localScale = Vector3.zero;
        this.replayBtnGroup.DOFade(0f, 0f);
        this.homeBtnGroup.DOFade(0f, 0f);
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

    public void SetActiveHomeButton(bool active)
    {
        this.homeButton.gameObject.SetActive(active);
    }

    public void SetActiveReplayButton(bool active)
    {
        this.replayButton.gameObject.SetActive(active);
    }

    private void ShowCurrentBuildInfo()
    {
        var buildInfoAsset = Resources.Load<TextAsset>("BuildInfo");
        if (buildInfoAsset == null)
        {
            this.buildInfoText.text = "<color=red>Missing BuildInfo.txt</color>";
            return;
        }

        var version = ExtractValue(buildInfoAsset.text, "App Version");
        var buildDate = ExtractValue(buildInfoAsset.text, "Build Date");
        this.buildInfoText.text = $"Version {version} | {buildDate}";
    }

    private static string ExtractValue(string content, string key)
    {
        foreach (var line in content.Split('\n'))
        {
            if (!line.StartsWith(key + ": ")) continue;
            return line.Substring(key.Length + 2)
                       .Replace("<color=green>", "")
                       .Replace("</color>", "")
                       .Trim();
        }
        return string.Empty;
    }
}