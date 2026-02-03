using Mimi.Events.AsyncBus;
using Mimi.Games.Events;
using Mimi.Persistence.LocalPrefs;
using Mimi.Prototypes;
using Mimi.Prototypes.SaveLoad;
using Mimi.Prototypes.UI;
using UnityEngine;

public class SettingViewPresenter : BaseViewPresenter
{
    private SettingView settingView;
    private readonly IAsyncPublisher eventPublisher;
    private readonly ILocalPrefs localPrefs;
    private readonly SettingModel settingModel;
    private readonly RuntimeState runtimeState;
    private readonly ISaveManager saveManager;
    private readonly IAudioService audioService;

    public SettingViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, SettingModel settingModel, ISaveManager saveManager,
        RuntimeState runtimeState, IAudioService audioService) : base(scenePresenter,
        transform)
    {
        this.eventPublisher = eventPublisher;
        this.settingModel = settingModel;
        this.saveManager = saveManager;
        this.runtimeState = runtimeState;
        this.audioService = audioService;
    }

    protected override void AddViews()
    {
        this.settingView = AddView<SettingView>();
    }

    protected override void AddChildren()
    {
    }

    protected override void OnShow()
    {
        base.OnShow();

        this.settingView.OnCloseClicked += CloseClickedHandler;
        this.settingView.OnMusicClicked += MusicClickedHandler;
        this.settingView.OnSoundClicked += SoundClickedHandler;
        this.settingView.OnVibrationClicked += VibrationClickedHandler;

        this.settingView.UpdateToggleValue(this.settingModel);
    }

    protected override void OnHide()
    {
        base.OnHide();

        this.settingView.OnCloseClicked -= CloseClickedHandler;
        this.settingView.OnMusicClicked -= MusicClickedHandler;
        this.settingView.OnSoundClicked -= SoundClickedHandler;
        this.settingView.OnVibrationClicked -= VibrationClickedHandler;

        this.saveManager.Save();
    }

    private void VibrationClickedHandler(bool value)
    {
        Debug.Log($"--- (SETTING) Vibration Setting changed to: {value}");
        this.settingModel.VibrationOn = value;
    }

    private void SoundClickedHandler(bool value)
    {
        Debug.Log($"--- (SETTING) Sound Setting changed to: {value}");
        this.settingModel.SoundOn = value;
        this.audioService.SetSoundVolPercentage(value ? 1 : 0);
    }

    private void MusicClickedHandler(bool value)
    {
        Debug.Log($"--- (SETTING) Music Setting changed to: {value}");
        this.settingModel.MusicOn = value;
        this.audioService.SetMusicVolPercentage(value ? 1 : 0);
    }

    private void CloseClickedHandler()
    {
        Debug.Log($"--- (SETTING) Close Setting Panel");

        int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
        this.eventPublisher.PublishAsync(new LevelResumed(currentLevelOrder.ToString()));
        Hide();
    }
}