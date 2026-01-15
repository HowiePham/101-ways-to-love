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

    private GameplayView gameplayView;
    private CoroutineHandle timerCoroutineHandler;
    private float timeLeft;
    private int maxProgress;
    private int currentProgress;

    private const float TimeStep = 1f;

    public GameplayViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, IAsyncSubscriber eventSubscriber, RuntimeState runtimeState) :
        base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.eventSubscriber = eventSubscriber;
        this.runtimeState = runtimeState;
    }

    protected override void AddViews()
    {
        this.gameplayView = AddView<GameplayView>();

        this.eventSubscriber.Subscribe<LevelResumed>(ResumeGameplay).AddToBag(this.eventBag);
    }

    protected override void AddChildren()
    {
    }

    protected override void OnShow()
    {
        base.OnShow();

        this.gameplayView.OnSettingClicked += SettingClickedHandler;
        this.gameplayView.OnSkipClicked += SkipClickedHandler;
        Messenger.AddListener(EventKey.LevelWin, ShowWinView);

        ShowLevelInfo();

#if DEVELOPMENT
        var cheatViewPresenter = this.ScenePresenter.GetViewPresenter<CheatViewPresenter>();
        cheatViewPresenter.Show();
#endif
    }

    private async UniTask ResumeGameplay(LevelResumed levelResumed, CancellationToken cancellationToken)
    {
    }

    protected override void OnHide()
    {
        base.OnHide();

        this.gameplayView.OnSettingClicked -= SettingClickedHandler;
        this.gameplayView.OnSkipClicked -= SkipClickedHandler;

        Messenger.RemoveListener(EventKey.LevelWin, ShowWinView);

#if DEVELOPMENT
        var cheatViewPresenter = this.ScenePresenter.GetViewPresenter<CheatViewPresenter>();
        cheatViewPresenter.Hide();
#endif
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