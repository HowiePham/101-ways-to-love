using System.Threading;
using _Modules._UI.CheatView.Scripts;
using _Modules._UI.WinView.Scripts;
using _Modules.GameEvent.Scripts;
using Cysharp.Threading.Tasks;
using FrogunnerGames;
using MEC;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Configs;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Games.Events;
using Mimi.Prototypes;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.UI;
using UnityEngine;

public class GameplayViewPresenter : BaseViewPresenter
{
    private readonly IAsyncPublisher eventPublisher;
    private readonly IAsyncSubscriber eventSubscriber;
    private readonly DisposableBag eventBag = new DisposableBag();
    private readonly RuntimeState runtimeState;
    private readonly LevelConfig hintLevelConfig;
    private readonly IConfigProvider remoteConfig;
    private readonly LifeSystem lifeSystem;
    private readonly IAdAdapter adAdapter;
    private readonly DialogManager dialogManager;
    private readonly ChapterLevelRepository chapterLevelRepo;
    private readonly ILevelOrder levelOrder;

    private GameplayView gameplayView;
    private TutorialOverlayView tutorialView;
    private NumberBasedLifeView numberBasedLifeView;
    private CoroutineHandle timerCoroutineHandler;
    private int previousLifeCount;
    private float timeLeft;
    private int maxProgress;
    private int currentProgress;
    private CancellationTokenSource tutorialCts;
    private bool isTutorialRunning;
    private int wrongAnswerCount;
    private bool isHintButtonShown;
    private bool isSkipButtonShown;
    private CancellationTokenSource hintButtonCts;
    private CancellationTokenSource skipButtonCts;

    private const string TutorialCompletedKey = "tutorial_overlay_completed_v1";

    public GameplayViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, IAsyncSubscriber eventSubscriber,
        RuntimeState runtimeState, LifeSystem lifeSystem, LevelConfig hintLevelConfig, IAdAdapter adAdapter, DialogManager dialogManager,
        ChapterLevelRepository chapterLevelRepo, ILevelOrder levelOrder, IConfigProvider remoteConfig) :
        base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.eventSubscriber = eventSubscriber;
        this.runtimeState = runtimeState;
        this.lifeSystem = lifeSystem;
        this.hintLevelConfig = hintLevelConfig;
        this.adAdapter = adAdapter;
        this.dialogManager = dialogManager;
        this.chapterLevelRepo = chapterLevelRepo;
        this.levelOrder = levelOrder;
        this.remoteConfig = remoteConfig;
    }

    protected override void AddViews()
    {
        this.gameplayView = AddView<GameplayView>();
        this.numberBasedLifeView = this.gameplayView.LifeView;
        this.tutorialView = AddView<TutorialOverlayView>(startingView: false);
    }

    protected override void AddChildren()
    {
    }

    protected override void OnShow()
    {
        base.OnShow();
        this.eventPublisher.PublishAsync(new ScreenShown("gameplay"));

        this.gameplayView.OnSettingClicked += SettingClickedHandler;
        this.gameplayView.OnSkipClicked += SkipClickedHandler;
        this.gameplayView.OnHintClicked += HintClickedHandler;
        this.gameplayView.OnRemoveAdsClicked += ShowRemoveAdsView;
        this.gameplayView.OnStartLevelGameClicked += StartLevelGameClickedHandler;
        this.numberBasedLifeView.Show();
        this.numberBasedLifeView.OnLifeButtonClicked += LifeButtonClickedHandler;
        this.gameplayView.OnNoLifeBlockerClicked += NoLifeBlockerClickedHandler;

        this.adAdapter.RewardVideo.OnRewarded += OnRewardCompleted;
        this.adAdapter.RewardVideo.OnShowFailed += OnRewardFailed;

        this.eventSubscriber.Subscribe<LevelResumed>(ResumeGameplay).AddToBag(this.eventBag);
        this.eventSubscriber.Subscribe<LifeUpdated>(OnLifeUpdate).AddToBag(this.eventBag);
        this.eventSubscriber.Subscribe<RecoveryLifeTimerUpdated>(OnRecoveryTimerUpdate).AddToBag(this.eventBag);
        Messenger.AddListener(EventKey.LevelWin, ShowWinView);
        Messenger.AddListener(EventKey.ActionDone, UpdateStepPoint);
        Messenger.AddListener(EventKey.ShowHint, ShowHint);
        Messenger.AddListener(EventKey.ActionFailed, ActionFailedHandler);
        Messenger.AddListener(EventKey.ShowStartLevelGameButton, ShowStartLevelGameButtonHandler);

        this.wrongAnswerCount = 0;
        this.isHintButtonShown = false;
        this.isSkipButtonShown = false;

        float hintButtonDelay = this.remoteConfig.GetValue(ConfigKey.HintButtonDelay).Float;
        float skipButtonDelay = this.remoteConfig.GetValue(ConfigKey.SkipButtonDelay).Float;
        HandleHintButtonVisible(hintButtonDelay);
        HandleSkipButtonVisible(skipButtonDelay);

        ShowLevelInfo();
        ShowChapterProgress();

        this.previousLifeCount = this.lifeSystem.CurrentLifeCount;
        this.numberBasedLifeView.SetLifeCount(this.lifeSystem.CurrentLifeCount);
        this.numberBasedLifeView.SetTimeRemaining(this.lifeSystem.GetRemainingTime());
        this.numberBasedLifeView.SetAddLifeIconActive(!this.lifeSystem.IsLifeIsFull());
        this.gameplayView.SetActiveNoLifeBlocker(!this.lifeSystem.AnyLifeLeft());

#if DEVELOPMENT
        var cheatViewPresenter = this.ScenePresenter.GetViewPresenter<CheatViewPresenter>();
        cheatViewPresenter.Show();
#endif

        TryStartTutorial();
    }

    private void TryStartTutorial()
    {
        if (this.tutorialView == null || !this.remoteConfig.GetValue(ConfigKey.ShowTutorialUI).Boolean) return;
        if (PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1) return;

        this.gameplayView.DelayProgressBarAnimation = true;

        this.tutorialCts?.Cancel();
        this.tutorialCts?.Dispose();
        this.tutorialCts = new CancellationTokenSource();
        RunTutorialSequence(this.tutorialCts.Token).Forget();
    }

    private async UniTaskVoid RunTutorialSequence(CancellationToken ct)
    {
        this.isTutorialRunning = true;
        Messenger.Broadcast(EventKey.PauseLevel, true);
        this.tutorialView.OnNextClicked += HandleTutorialNextClicked;
        this.tutorialView.Show();

        // Wait for GameplayView entry animations to finish
        bool canceled = await UniTask.Delay(300, cancellationToken: ct).SuppressCancellationThrow();
        if (canceled)
        {
            CleanupTutorial();
            return;
        }

        // Progress bar plays first while screen is still fully visible
        await this.gameplayView.PlayChapterProgressBarAnimation(autoHide: false);
        if (ct.IsCancellationRequested)
        {
            CleanupTutorial();
            return;
        }

        // Dark overlay fades in after progress bar has finished its animation
        await this.tutorialView.PlayIntroAnimation(ct);
        if (ct.IsCancellationRequested)
        {
            CleanupTutorial();
            return;
        }

        TutorialStepData[] steps = this.tutorialView.Steps;
        for (int i = 0; i < steps.Length; i++)
        {
            if (ct.IsCancellationRequested)
            {
                CleanupTutorial();
                return;
            }

            await this.tutorialView.TransitionToStep(steps[i], ct);
            if (ct.IsCancellationRequested)
            {
                CleanupTutorial();
                return;
            }

            await WaitForTutorialNext(ct);
            if (ct.IsCancellationRequested)
            {
                CleanupTutorial();
                return;
            }
        }

        await this.tutorialView.PlayOutroAnimation(ct);

        PlayerPrefs.SetInt(TutorialCompletedKey, 1);
        PlayerPrefs.Save();
        CleanupTutorial();
    }

    private UniTaskCompletionSource tutorialNextSource;

    private UniTask WaitForTutorialNext(CancellationToken ct)
    {
        this.tutorialNextSource = new UniTaskCompletionSource();
        return this.tutorialNextSource.Task.AttachExternalCancellation(ct);
    }

    private void HandleTutorialNextClicked()
    {
        this.tutorialNextSource?.TrySetResult();
    }

    private void CleanupTutorial()
    {
        this.isTutorialRunning = false;
        this.tutorialView.OnNextClicked -= HandleTutorialNextClicked;
        this.tutorialView.Hide();
        this.gameplayView.HideChapterProgressBar().Forget();
        Messenger.Broadcast(EventKey.PauseLevel, false);
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
        if (this.isSkipButtonShown) return;

        this.skipButtonCts?.Cancel();
        this.skipButtonCts?.Dispose();
        this.skipButtonCts = new CancellationTokenSource();
        ScheduleSkipButton(delay, this.skipButtonCts.Token).Forget();
    }

    private async UniTaskVoid ScheduleSkipButton(float delay, CancellationToken ct)
    {
        if (delay > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: ct);
        if (ct.IsCancellationRequested) return;
        this.isSkipButtonShown = true;
        this.gameplayView.SetActiveSkipButton(true).Forget();
    }

    private void HandleHintButtonVisible(float delay = 0)
    {
        if (this.isHintButtonShown) return;
        int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
        bool isHintLevel = this.hintLevelConfig.HasLevel(currentLevelOrder.ToString());
        if (isHintLevel) return;

        this.hintButtonCts?.Cancel();
        this.hintButtonCts?.Dispose();
        this.hintButtonCts = new CancellationTokenSource();
        ScheduleHintButton(delay, this.hintButtonCts.Token).Forget();
    }

    private async UniTaskVoid ScheduleHintButton(float delay, CancellationToken ct)
    {
        if (delay > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: ct);
        if (ct.IsCancellationRequested) return;
        this.isHintButtonShown = true;
        this.gameplayView.SetActiveHintButton(true).Forget();
    }

    public void SetDelayProgressBarAnimation(bool delay)
    {
        this.gameplayView.DelayProgressBarAnimation = delay;
    }

    public async UniTask PlayChapterProgressBarAnimation()
    {
        await this.gameplayView.PlayChapterProgressBarAnimation();
    }

    public void InitStepPoint(int stepNumber)
    {
        this.gameplayView.InitStepPoint(stepNumber);
    }

    private void HintClickedHandler()
    {
        Debug.Log($"--- (HINT) Hint button clicked --- Reward ready: {this.adAdapter.RewardVideo.IsReady}");
        if (this.adAdapter.RewardVideo.IsReady)
        {
            Debug.Log($"--- (ADS) Showing ads for hint");

            this.eventPublisher.PublishAsync(new AdShowRequested());
            this.adAdapter.RewardVideo.Show(new AdReward("hint"), new AdPlacement("gameplay"));
        }
        else
        {
            ShowAdFailedDialog();
        }
    }

    private void OnRewardCompleted(AdReward reward)
    {
        string rewardRewardId = reward.RewardId;
        Debug.Log($"--- (ADS) GameplayView Reward: {rewardRewardId}");

        switch (rewardRewardId)
        {
            case "hint":
                ShowHint();
                break;
            case "skip_level":
                this.eventPublisher.PublishAsync(new SkipLevel());
                break;
        }
    }

    private void ShowHint()
    {
        this.gameplayView.SetActiveHintButton(false);
        this.eventPublisher.PublishAsync(new UseHint());
    }

    private void OnRewardFailed(AdReward adReward, AdError adError)
    {
        ShowAdFailedDialog();
    }

    private void ShowAdFailedDialog()
    {
        if (this.dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide,
                out AutoHideNotificationDialog dialog))
        {
            dialog.SetText("Ads is not available");
        }
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
        this.hintButtonCts?.Cancel();
        this.hintButtonCts?.Dispose();
        this.hintButtonCts = null;

        this.skipButtonCts?.Cancel();
        this.skipButtonCts?.Dispose();
        this.skipButtonCts = null;

        base.OnHide();

        if (this.isTutorialRunning)
        {
            this.tutorialCts?.Cancel();
            this.tutorialCts?.Dispose();
            this.tutorialCts = null;
            this.isTutorialRunning = false;
            this.tutorialView.OnNextClicked -= HandleTutorialNextClicked;
            Messenger.Broadcast(EventKey.PauseLevel, false);
        }

        this.gameplayView.SetActiveStartLevelGameButton(false);

        this.gameplayView.OnSettingClicked -= SettingClickedHandler;
        this.gameplayView.OnSkipClicked -= SkipClickedHandler;
        this.gameplayView.OnHintClicked -= HintClickedHandler;
        this.gameplayView.OnRemoveAdsClicked -= ShowRemoveAdsView;
        this.gameplayView.OnStartLevelGameClicked -= StartLevelGameClickedHandler;
        this.numberBasedLifeView.OnLifeButtonClicked -= LifeButtonClickedHandler;
        this.numberBasedLifeView.Hide();
        this.gameplayView.OnNoLifeBlockerClicked -= NoLifeBlockerClickedHandler;

        this.adAdapter.RewardVideo.OnRewarded -= OnRewardCompleted;
        this.adAdapter.RewardVideo.OnShowFailed -= OnRewardFailed;

        this.eventBag.Dispose();
        Messenger.RemoveListener(EventKey.LevelWin, ShowWinView);
        Messenger.RemoveListener(EventKey.ActionDone, UpdateStepPoint);
        Messenger.RemoveListener(EventKey.ShowHint, ShowHint);
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

        if (currentLifeCount < this.previousLifeCount)
        {
            this.numberBasedLifeView.PlayLifeLostEffect();
            await UniTask.Delay(200, cancellationToken: cancellationToken);
        }
        else if (currentLifeCount > this.previousLifeCount)
        {
            this.numberBasedLifeView.PlayLifeGainedEffect();
            await UniTask.Delay(200, cancellationToken: cancellationToken);
        }

        this.previousLifeCount = currentLifeCount;
        this.numberBasedLifeView.SetLifeCount(currentLifeCount);
        this.numberBasedLifeView.SetAddLifeIconActive(!this.lifeSystem.IsLifeIsFull());
        this.gameplayView.SetActiveNoLifeBlocker(currentLifeCount <= 0);
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

    private void ShowChapterProgress()
    {
        int currentOrder = this.runtimeState.CurrentLevelOrder.Value;
        LevelInfo currentLevel = this.levelOrder.GetByOrder(currentOrder);
        if (currentLevel == null) return;

        ChapterInfo chapter = this.chapterLevelRepo.GetChapter(currentLevel.Chapter);
        if (chapter == null) return;

        int firstStage = chapter.Levels[0].StageNumber;
        int currentStage = currentOrder + 1;
        int completedInChapter = currentStage - firstStage + 1;

        this.gameplayView.SetChapterProgress(completedInChapter, chapter.LevelCount);
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
        this.eventPublisher.PublishAsync(new LifeUsing("action_failed"));
        this.eventPublisher.PublishAsync(new ActionFailedMessage());
        HandleFailedUI();
    }

    private async UniTask HandleFailedUI()
    {
        await this.gameplayView.ShowWrongSignal();
        int wrongCountForHint = this.remoteConfig.GetValue(ConfigKey.ShowHintButtonAfterWrongTimes).Int;
        int wrongCountForSkip = this.remoteConfig.GetValue(ConfigKey.ShowSkipButtonAfterWrongTimes).Int;

        this.wrongAnswerCount++;
        if (this.wrongAnswerCount >= wrongCountForHint)
            HandleHintButtonVisible(0f);
        if (this.wrongAnswerCount >= wrongCountForSkip)
            HandleSkipButtonVisible(0f);
    }

    private void SettingClickedHandler()
    {
        Debug.Log($"--- (GAMEVIEW) Click setting");

        var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
        settingViewPresenter.Show();
        settingViewPresenter.SetActiveReplayButton(true);
        settingViewPresenter.SetActiveHomeButton(true);
        Messenger.Broadcast(EventKey.PauseLevel, true);
    }

    private void SkipClickedHandler()
    {
        if (this.adAdapter.RewardVideo.IsReady)
        {
            this.eventPublisher.PublishAsync(new AdShowRequested());
            this.adAdapter.RewardVideo.Show(new AdReward("skip_level"), new AdPlacement("gameplay"));
        }
        else
        {
            ShowAdFailedDialog();
        }
    }

    private void LifeButtonClickedHandler()
    {
        this.lifeSystem.ShowGetMoreLifeDialog(DialogId.GetMoreLifeDialog);
    }

    private void NoLifeBlockerClickedHandler()
    {
        this.lifeSystem.ShowGetMoreLifeDialog(DialogId.EndOfLifeDialog);
    }

    private void ShowRemoveAdsView()
    {
        var removeAdsPresenter = this.ScenePresenter.GetViewPresenter<RemoveAdsViewPresenter>();
        removeAdsPresenter.Show();
    }
}