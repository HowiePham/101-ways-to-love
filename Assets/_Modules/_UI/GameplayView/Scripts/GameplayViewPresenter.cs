using System.Threading;
using _Modules._UI.WinView.Scripts;
using Cysharp.Threading.Tasks;
using MEC;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Games.Events;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.UI;
using UnityEngine;

public class GameplayViewPresenter : BaseViewPresenter
{
    private readonly IAsyncPublisher eventPublisher;
    private readonly IAsyncSubscriber eventSubscriber;
    private readonly DisposableBag eventBag = new DisposableBag();

    private GameplayView gameplayView;
    private CoroutineHandle timerCoroutineHandler;
    private float timeLeft;
    private int maxProgress;
    private int currentProgress;

    private const float TimeStep = 1f;

    public GameplayViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, IAsyncSubscriber eventSubscriber) : base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.eventSubscriber = eventSubscriber;
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

        this.gameplayView.OnPauseClicked += PauseClickedHandler;
        Messenger.AddListener(EventKey.LevelWin, ShowWinView);

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

        this.gameplayView.OnPauseClicked -= PauseClickedHandler;
        Messenger.RemoveListener(EventKey.LevelWin, ShowWinView);

#if DEVELOPMENT
        var cheatViewPresenter = this.ScenePresenter.GetViewPresenter<CheatViewPresenter>();
        cheatViewPresenter.Hide();
#endif
    }

    private void ShowWinView()
    {
        var winViewPresenter = this.ScenePresenter.GetViewPresenter<WinViewPresenter>();
        winViewPresenter.Show();

        Hide();
    }

    private void PauseClickedHandler()
    {
        Debug.Log($"--- (GAMEVIEW) Click Pause");
        
        var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
        settingViewPresenter.Show();
    }
}