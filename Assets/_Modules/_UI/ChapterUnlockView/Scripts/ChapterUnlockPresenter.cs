using _Modules.GameEvent.Scripts;
using Mimi;
using Mimi.Audio;
using Mimi.Configs;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes;
using Mimi.Prototypes.Currencies;
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

    public bool TryShowForNextChapter()
    {
        if (!this.runtimeState.IsNewChapterUnlocked.Value) return false;

        int currentOrder = this.runtimeState.CurrentLevelOrder.Value;
        LevelInfo nextLevel = this.levelOrder.GetNextLevel(currentOrder);
        if (nextLevel == null) return false;

        ShowForChapter(nextLevel.Chapter);
        return true;
    }

    public void ShowForChapter(int chapterNumber)
    {
        this.eventPublisher.PublishAsync(new DestroyLevelRequested());

        ChapterInfo chapter = this.chapterLevelRepo.GetChapter(chapterNumber);
        Sprite icon = Resources.Load<Sprite>("Icons/" + chapter.ChapterIconAddress);
        this.chapterUnlockView.SetChapterData(icon, "Chapter " + chapter.ChapterNumber + ": " + chapter.ChapterName);
        Show();
    }

    protected override void OnShow()
    {
        base.OnShow();

        this.chapterUnlockView.OnContinueClicked += ContinueClickedHandler;
        this.chapterUnlockView.OnBackHomeClicked += BackHomeClickedHandler;
        this.chapterUnlockView.OnShowingCompleted += ShowingCompletedHandler;
    }

    protected override void OnHide()
    {
        base.OnHide();

        this.chapterUnlockView.OnContinueClicked -= ContinueClickedHandler;
        this.chapterUnlockView.OnBackHomeClicked -= BackHomeClickedHandler;
        this.chapterUnlockView.OnShowingCompleted -= ShowingCompletedHandler;
    }

    private void ShowingCompletedHandler()
    {
        if (this.dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide, out AutoHideNotificationDialog dialog))
        {
            int rewardValue = this.remoteConfig.GetValue(ConfigKey.LifeRecoverAfterChapter).Int;
            dialog.SetText("+" + rewardValue + " <sprite index=0>");
        }
    }

    private void ContinueClickedHandler()
    {
        this.eventPublisher.PublishAsync(new NextLevelClicked());
        Hide();
    }

    private void BackHomeClickedHandler()
    {
        this.eventPublisher.PublishAsync(new BackHome());
        Hide();
    }
}