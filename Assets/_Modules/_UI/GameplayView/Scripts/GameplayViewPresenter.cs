using System.Threading;
using _Modules._UI.CheatView.Scripts;
using _Modules._UI.WinView.Scripts;
using Cysharp.Threading.Tasks;
using MEC;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Games.Events;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.UI;
using Mimi.Rx.Variables;
using UnityEngine;

public class GameplayViewPresenter : BaseViewPresenter
{
    private readonly IAsyncPublisher eventPublisher;
    private readonly IAsyncSubscriber eventSubscriber;
    private readonly DisposableBag eventBag = new DisposableBag();
    private readonly RuntimeState runtimeState;
    private readonly LifeSystem lifeSystem;

    private GameplayView gameplayView;
    private NumberBasedLifeView numberBasedLifeView;
    private CoroutineHandle timerCoroutineHandler;
    private float timeLeft;
    private int maxProgress;
    private int currentProgress;

    private const float TimeStep = 1f;

    public GameplayViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, IAsyncSubscriber eventSubscriber, RuntimeState runtimeState,
        LifeSystem lifeSystem) :
        base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.eventSubscriber = eventSubscriber;
        this.runtimeState = runtimeState;
        this.lifeSystem = lifeSystem;
    }

    protected override void AddViews()
    {
        this.gameplayView = AddView<GameplayView>();
        this.numberBasedLifeView = AddView<NumberBasedLifeView>();
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

        this.eventSubscriber.Subscribe<LevelResumed>(ResumeGameplay).AddToBag(this.eventBag);
        this.eventSubscriber.Subscribe<LifeUpdated>(OnLifeUpdate).AddToBag(this.eventBag);
        this.eventSubscriber.Subscribe<RecoveryLifeTimerUpdated>(OnRecoveryTimerUpdate).AddToBag(this.eventBag);
        Messenger.AddListener(EventKey.LevelWin, ShowWinView);
        Messenger.AddListener(EventKey.ActionDone, UpdateStepPoint);
        Messenger.AddListener(EventKey.ActionFailed, ActionFailedHandler);

        ShowLevelInfo();

        // this.numberBasedLifeView.SetLifeCount(this.lifeSystem.CurrentLifeCount);
        // this.numberBasedLifeView.SetTimeRemaining(this.lifeSystem.GetRemainingTime());

#if DEVELOPMENT
        var cheatViewPresenter = this.ScenePresenter.GetViewPresenter<CheatViewPresenter>();
        cheatViewPresenter.Show();
#endif
    }

    public void InitStepPoint(int stepNumber)
    {
        this.gameplayView.InitStepPoint(stepNumber);
    }

    private void HintClickedHandler()
    {
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

        this.gameplayView.OnSettingClicked -= SettingClickedHandler;
        this.gameplayView.OnSkipClicked -= SkipClickedHandler;
        this.gameplayView.OnHintClicked -= HintClickedHandler;

        this.eventBag.Dispose();
        Messenger.RemoveListener(EventKey.LevelWin, ShowWinView);
        Messenger.RemoveListener(EventKey.ActionDone, UpdateStepPoint);
        Messenger.RemoveListener(EventKey.ActionFailed, ActionFailedHandler);

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
        winViewPresenter.Show();

        settingViewPresenter.Hide();
        Hide();
    }

    private void ActionFailedHandler()
    {
        // this.gameplayView.ShowWrongSignal();
        // this.eventPublisher.PublishAsync(new LifeUsing());
    }

    private void SettingClickedHandler()
    {
        Debug.Log($"--- (GAMEVIEW) Click setting");

        var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
        settingViewPresenter.Show();
    }

    private void SkipClickedHandler()
    {
        this.eventPublisher.PublishAsync(new NextLevelClicked());
    }
}