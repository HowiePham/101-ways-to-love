using System.Threading;
using _Modules._UI.CheatView.Scripts;
using _Modules._UI.WinView.Scripts;
using Cysharp.Threading.Tasks;
using MEC;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Games.Events;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.UI;
using UnityEngine;

public class GameplayViewPresenter : BaseViewPresenter
{
    private readonly IAsyncPublisher eventPublisher;
    private readonly IAsyncSubscriber eventSubscriber;
    private readonly DisposableBag eventBag = new DisposableBag();
    private readonly RuntimeState runtimeState;
    private readonly LevelConfig hintLevelConfig;
    private readonly LifeSystem lifeSystem;

    private GameplayView gameplayView;
    private NumberBasedLifeView numberBasedLifeView;
    private CoroutineHandle timerCoroutineHandler;
    private float timeLeft;
    private int maxProgress;
    private int currentProgress;

    private const float TimeStep = 1f;

    public GameplayViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, IAsyncSubscriber eventSubscriber,
        RuntimeState runtimeState, LifeSystem lifeSystem, LevelConfig hintLevelConfig) :
        base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.eventSubscriber = eventSubscriber;
        this.runtimeState = runtimeState;
        this.lifeSystem = lifeSystem;
        this.hintLevelConfig = hintLevelConfig;
    }

    protected override void AddViews()
    {
        this.gameplayView = AddView<GameplayView>();
        // this.numberBasedLifeView = AddView<NumberBasedLifeView>();
    }

    protected override void AddChildren()
    {
    }

    protected override void OnShow()
    {
        base.OnShow();

        this.gameplayView.OnSettingClicked += SettingClickedHandler;
        this.gameplayView.OnSkipClicked += SkipClickedHandler;
        this.gameplayView.OnHintClicked += HintClickedHandler;
        this.gameplayView.OnRemoveAdsClicked += ShowRemoveAdsView;
        this.gameplayView.OnStartLevelGameClicked += StartLevelGameClickedHandler;

        this.eventSubscriber.Subscribe<LevelResumed>(ResumeGameplay).AddToBag(this.eventBag);
        this.eventSubscriber.Subscribe<LifeUpdated>(OnLifeUpdate).AddToBag(this.eventBag);
        this.eventSubscriber.Subscribe<RecoveryLifeTimerUpdated>(OnRecoveryTimerUpdate).AddToBag(this.eventBag);
        Messenger.AddListener(EventKey.LevelWin, ShowWinView);
        Messenger.AddListener(EventKey.ActionDone, UpdateStepPoint);
        Messenger.AddListener(EventKey.ShowHint, HintClickedHandler);
        Messenger.AddListener(EventKey.ActionFailed, ActionFailedHandler);
        Messenger.AddListener(EventKey.ShowStartLevelGameButton, ShowStartLevelGameButtonHandler);

        HandleHintButtonVisible(3f);
        HandleSkipButtonVisible(5f);
        ShowLevelInfo();

        // this.numberBasedLifeView.SetLifeCount(this.lifeSystem.CurrentLifeCount);
        // this.numberBasedLifeView.SetTimeRemaining(this.lifeSystem.GetRemainingTime());

#if DEVELOPMENT
        var cheatViewPresenter = this.ScenePresenter.GetViewPresenter<CheatViewPresenter>();
        cheatViewPresenter.Show();
#endif
    }

    private void ShowStartLevelGameButtonHandler()
    {
        this.gameplayView.SetActiveStartLevelGameButton(true);
    }

    private void StartLevelGameClickedHandler()
    {
        Messenger.Broadcast(EventKey.StartLevelGame);
        this.gameplayView.SetActiveStartLevelGameButton(false);
    }

    private void HandleSkipButtonVisible(float delay = 0)
    {
        this.gameplayView.SetActiveSkipButton(true, delay);
    }

    private void HandleHintButtonVisible(float delay = 0)
    {
        int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
        bool isHintLevel = this.hintLevelConfig.HasLevel(currentLevelOrder.ToString());
        this.gameplayView.SetActiveHintButton(!isHintLevel, delay);
    }

    public void InitStepPoint(int stepNumber)
    {
        this.gameplayView.InitStepPoint(stepNumber);
    }

    private void HintClickedHandler()
    {
        this.gameplayView.SetActiveHintButton(false);
        this.eventPublisher.PublishAsync(new UseHint());
    }

    private async UniTask ResumeGameplay(LevelResumed levelResumed, CancellationToken cancellationToken)
    {
    }

    private void UpdateStepPoint()
    {
        this.gameplayView.UpdateStepPoint();
    }

    protected override void OnHide()
    {
        base.OnHide();
        this.gameplayView.SetActiveStartLevelGameButton(false);

        this.gameplayView.OnSettingClicked -= SettingClickedHandler;
        this.gameplayView.OnSkipClicked -= SkipClickedHandler;
        this.gameplayView.OnHintClicked -= HintClickedHandler;
        this.gameplayView.OnRemoveAdsClicked -= ShowRemoveAdsView;
        this.gameplayView.OnStartLevelGameClicked -= StartLevelGameClickedHandler;

        this.eventBag.Dispose();
        Messenger.RemoveListener(EventKey.LevelWin, ShowWinView);
        Messenger.RemoveListener(EventKey.ActionDone, UpdateStepPoint);
        Messenger.RemoveListener(EventKey.ShowHint, HintClickedHandler);
        Messenger.RemoveListener(EventKey.ActionFailed, ActionFailedHandler);
        Messenger.RemoveListener(EventKey.ShowStartLevelGameButton, ShowStartLevelGameButtonHandler);

#if DEVELOPMENT
        var cheatViewPresenter = this.ScenePresenter.GetViewPresenter<CheatViewPresenter>();
        cheatViewPresenter.Hide();
#endif
    }

    private async UniTask OnLifeUpdate(LifeUpdated lifeUpdated, CancellationToken cancellationToken)
    {
        int currentLifeCount = lifeUpdated.LifeCount;
        this.numberBasedLifeView.SetLifeCount(currentLifeCount);
        await UniTask.CompletedTask;
    }

    private async UniTask OnRecoveryTimerUpdate(RecoveryLifeTimerUpdated recoveryLifeTimerUpdated, CancellationToken cancellationToken)
    {
        string timeRemaining = recoveryLifeTimerUpdated.RemainingTime;
        this.numberBasedLifeView.SetTimeRemaining(timeRemaining);
        await UniTask.CompletedTask;
    }

    private void ShowLevelInfo()
    {
        int currentLevel = this.runtimeState.CurrentLevelOrder.Value + 1;
        this.gameplayView.SetLevelCurrent(currentLevel.ToString());
    }

    private void ShowWinView()
    {
        Debug.Log($"--- (GAMEVIEW) Show Win View");

        var winViewPresenter = this.ScenePresenter.GetViewPresenter<WinViewPresenter>();
        var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
        var hardLevelViewPresenter = this.ScenePresenter.GetViewPresenter<HardLevelViewPresenter>();
        winViewPresenter.Show();

        settingViewPresenter.Hide();
        hardLevelViewPresenter.Hide();
        Hide();
    }

    private void ActionFailedHandler()
    {
        this.gameplayView.ShowWrongSignal();
        // this.eventPublisher.PublishAsync(new LifeUsing());
    }

    private void SettingClickedHandler()
    {
        Debug.Log($"--- (GAMEVIEW) Click setting");

        var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
        settingViewPresenter.Show();
        Messenger.Broadcast(EventKey.PauseLevel);
    }

    private void SkipClickedHandler()
    {
        this.eventPublisher.PublishAsync(new SkipLevel());
    }

    private void ShowRemoveAdsView()
    {
        var removeAdsPresenter = this.ScenePresenter.GetViewPresenter<RemoveAdsViewPresenter>();
        removeAdsPresenter.Show();
    }
}