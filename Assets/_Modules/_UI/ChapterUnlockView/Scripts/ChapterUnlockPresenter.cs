using _Modules.GameEvent.Scripts;
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
    private readonly RuntimeState runtimeState;

    public ChapterUnlockPresenter(BaseScenePresenter scenePresenter, Transform transform,
        IAsyncPublisher eventPublisher, ChapterLevelRepository chapterLevelRepo,
        ILevelOrder levelOrder, RuntimeState runtimeState) : base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.chapterLevelRepo = chapterLevelRepo;
        this.levelOrder = levelOrder;
        this.runtimeState = runtimeState;
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
        int currentOrder = this.runtimeState.CurrentLevelOrder.Value;

        LevelInfo currentLevel = this.levelOrder.GetByOrder(currentOrder);
        if (currentLevel == null) return false;

        LevelInfo nextLevel = this.levelOrder.GetNextLevel(currentOrder);
        if (nextLevel == null) return false;

        if (nextLevel.Chapter != currentLevel.Chapter)
        {
            int nextOrder = currentOrder + 1;
            int topLevel = this.runtimeState.TopLevelOrder.Value;
            if (nextOrder >= topLevel)
            {
                ShowForChapter(nextLevel.Chapter);
                return true;
            }
        }

        return false;
    }

    public void ShowForChapter(int chapterNumber)
    {
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
    }

    protected override void OnHide()
    {
        base.OnHide();

        this.chapterUnlockView.OnContinueClicked -= ContinueClickedHandler;
        this.chapterUnlockView.OnBackHomeClicked -= BackHomeClickedHandler;
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
