using _Modules.GameEvent.Scripts;
using Cysharp.Threading.Tasks;
using Mimi.Configs;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.UI;
using UnityEngine;

public class ChapterUnlockPresenter : BaseViewPresenter
{
    private ChapterUnlockView chapterUnlockView;
    private readonly IAsyncPublisher eventPublisher;
    private readonly ChapterLevelRepository chapterLevelRepo;
    private readonly ILevelOrder levelOrder;
    private readonly IConfigProvider remoteConfig;
    private readonly DialogManager dialogManager;
    private readonly RuntimeState runtimeState;
    private bool suppressEventPublish;
    private bool backHomeRequested;

    public bool WasBackHomeRequested => this.backHomeRequested;

    public ChapterUnlockPresenter(BaseScenePresenter scenePresenter, Transform transform,
        IAsyncPublisher eventPublisher, ChapterLevelRepository chapterLevelRepo,
        ILevelOrder levelOrder, RuntimeState runtimeState, IConfigProvider remoteConfig, DialogManager dialogManager) : base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.chapterLevelRepo = chapterLevelRepo;
        this.levelOrder = levelOrder;
        this.runtimeState = runtimeState;
        this.remoteConfig = remoteConfig;
        this.dialogManager = dialogManager;
    }

    protected override void AddViews()
    {
        this.chapterUnlockView = AddView<ChapterUnlockView>();
    }

    protected override void AddChildren()
    {
    }

    public async UniTask ShowForFirstSession(int chapterNumber)
    {
        ChapterInfo chapter = this.chapterLevelRepo.GetChapter(chapterNumber);
        Sprite icon = Resources.Load<Sprite>("Icons/" + chapter.ChapterIconAddress);
        this.chapterUnlockView.SetChapterData(icon, chapter.ChapterName);

        await this.chapterUnlockView.ShowAuto();
        await UniTask.WaitForSeconds(1f);

        this.chapterUnlockView.Hide();
    }

    public bool TryShowForNextChapter()
    {
        if (!this.runtimeState.IsNewChapterUnlocked.Value) return false;

        int currentOrder = this.runtimeState.CurrentLevelOrder.Value;
        LevelInfo nextLevel = this.levelOrder.GetNextLevel(currentOrder);
        if (nextLevel == null) return false;

        ShowForFirstSession(nextLevel.Chapter).Forget();
        return true;
    }

    public async UniTask<bool> TryShowForNextChapterAndWait()
    {
        if (!this.runtimeState.IsNewChapterUnlocked.Value) return false;

        int currentOrder = this.runtimeState.CurrentLevelOrder.Value;
        LevelInfo nextLevel = this.levelOrder.GetNextLevel(currentOrder);
        if (nextLevel == null) return false;

        this.eventPublisher.PublishAsync(new DestroyLevelRequested());
        await ShowForFirstSession(nextLevel.Chapter);
        return true;
    }

    public void ShowForChapter(int chapterNumber)
    {
        ShowForFirstSession(chapterNumber).Forget();
    }

    protected override void OnShow()
    {
        base.OnShow();
        this.eventPublisher.PublishAsync(new ScreenShown("chapter_unlock"));

        this.chapterUnlockView.OnContinueClicked += ContinueClickedHandler;
        this.chapterUnlockView.OnBackHomeClicked += BackHomeClickedHandler;
    }

    protected override void OnHide()
    {
        base.OnHide();

        this.chapterUnlockView.OnContinueClicked -= ContinueClickedHandler;
        this.chapterUnlockView.OnBackHomeClicked -= BackHomeClickedHandler;
    }

    private void ContinueClickedHandler()
    {
        Hide();
        if (!this.suppressEventPublish)
            this.eventPublisher.PublishAsync(new NextLevelClicked());
    }

    private void BackHomeClickedHandler()
    {
        this.backHomeRequested = true;
        Hide();
        if (!this.suppressEventPublish)
            this.eventPublisher.PublishAsync(new BackHome());
    }
}