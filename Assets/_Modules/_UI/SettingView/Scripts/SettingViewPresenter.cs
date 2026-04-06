using _Modules._UI.LoseView.Scripts;
using _Modules.GameEvent.Scripts;
using Mimi.Ads.Adapters;
using Mimi.Events.AsyncBus;
using Mimi.Games.Events;
using Mimi.Persistence.LocalPrefs;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
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
    private readonly IAdAdapter adsAdapter;
    private readonly IAudioService audioService;

    public SettingViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, SettingModel settingModel, ISaveManager saveManager,
        RuntimeState runtimeState, IAudioService audioService, IAdAdapter adsAdapter) : base(scenePresenter,
        transform)
    {
        this.eventPublisher = eventPublisher;
        this.settingModel = settingModel;
        this.saveManager = saveManager;
        this.runtimeState = runtimeState;
        this.audioService = audioService;
        this.adsAdapter = adsAdapter;
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
        this.settingView.OnReplayClicked += ReplayClickedHandler;
        this.settingView.OnHomeClicked += HomeClickedHandler;
        this.settingView.OnMusicClicked += MusicClickedHandler;
        this.settingView.OnSoundClicked += SoundClickedHandler;
        this.settingView.OnVibrationClicked += VibrationClickedHandler;

        this.settingView.UpdateToggleValue(this.settingModel);
        this.adsAdapter.Mrec.Show(new AdPlacement("setting_view"));
    }

    protected override void OnHide()
    {
        base.OnHide();

        this.settingView.OnCloseClicked -= CloseClickedHandler;
        this.settingView.OnReplayClicked -= ReplayClickedHandler;
        this.settingView.OnHomeClicked -= HomeClickedHandler;
        this.settingView.OnMusicClicked -= MusicClickedHandler;
        this.settingView.OnSoundClicked -= SoundClickedHandler;
        this.settingView.OnVibrationClicked -= VibrationClickedHandler;

        this.saveManager.Save();
        this.adsAdapter.Mrec.Hide();
        Messenger.Broadcast(EventKey.PauseLevel, false);
    }

    public void SetActiveHomeButton(bool active)
    {
        this.settingView.SetActiveHomeButton(active);
    }

    public void SetActiveReplayButton(bool active)
    {
        this.settingView.SetActiveReplayButton(active);
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

    private void ReplayClickedHandler()
    {
        Debug.Log($"--- (SETTING) Replay Level");
        HideGameplayViews();
        this.eventPublisher.PublishAsync(new LevelTryAgain());
        Hide();
    }

    private void HomeClickedHandler()
    {
        Debug.Log($"--- (SETTING) Back Home");
        HideGameplayViews();
        this.eventPublisher.PublishAsync(new BackHome());
        Hide();
    }

    private void HideGameplayViews()
    {
        var gameplayViewPresenter = this.ScenePresenter.GetViewPresenter<GameplayViewPresenter>();
        if (gameplayViewPresenter.IsShowing)
        {
            gameplayViewPresenter.Hide();
        }

        var hardLevelViewPresenter = this.ScenePresenter.GetViewPresenter<HardLevelViewPresenter>();
        if (hardLevelViewPresenter.IsShowing)
        {
            hardLevelViewPresenter.Hide();
        }
    }
}