using System.Collections.Generic;
using System.Linq;
using _Modules.Gameflow_Events_.Scripts;
using EnhancedUI.EnhancedScroller;
using Games;
using Mimi.Ads.Adapters;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.UI;
using UnityEngine;

public class ChapterSelectLevelPresenter : BaseViewPresenter
{
    private ChapterSelectLevelView chapterView;

    private readonly ChapterLevelRepository chapterLevelRepo;
    private readonly ILevelOrder levelOrder;
    private readonly IAudioService audioService;
    private readonly IAdAdapter adAdapter;
    private readonly IAsyncPublisher eventPublisher;
    private readonly DisposableBag disposeBag;
    private readonly RuntimeState runtimeState;
    private readonly List<ChapterInfo> listChapter;
    private int currentPageOrder = 1;

    public ChapterSelectLevelPresenter(BaseScenePresenter scenePresenter, Transform transform,
        ILevelRepository levelRepository, ILevelOrder levelOrder,
        IAudioService audioService, IAdAdapter adAdapter, RuntimeState runtimeState,
        IAsyncPublisher eventPublisher, List<SheetChapterModel> chapterModels)
        : base(scenePresenter, transform)
    {
        this.chapterLevelRepo = new ChapterLevelRepository(levelRepository, chapterModels);
        this.levelOrder = levelOrder;
        this.runtimeState = runtimeState;
        this.audioService = audioService;
        this.adAdapter = adAdapter;
        this.eventPublisher = eventPublisher;
        this.disposeBag = new DisposableBag();

        this.listChapter = new List<ChapterInfo>(this.chapterLevelRepo.GetChapters());
    }

    protected override void AddViews()
    {
        this.chapterView = AddView<ChapterSelectLevelView>();
    }

    protected override void OnShow()
    {
        base.OnShow();

        this.chapterView.OnClickSetting += OnClickSettingHandler;
        this.chapterView.OnTopButtonClick += JumpToFirstPage;
        this.chapterView.OnBottomButtonClick += JumpToLastPage;

        Messenger.AddListener<ChapterCellView>(EventKey.SelectChapter, OnChapterCellSelected);

        this.adAdapter.Mrec.Hide();
        ReloadChapterSelectionPage();
    }

    protected override void OnHide()
    {
        base.OnHide();

        this.chapterView.OnClickSetting -= OnClickSettingHandler;
        this.chapterView.OnTopButtonClick -= JumpToFirstPage;
        this.chapterView.OnBottomButtonClick -= JumpToLastPage;

        Messenger.RemoveListener<ChapterCellView>(EventKey.SelectChapter, OnChapterCellSelected);

        this.disposeBag.Dispose();
        this.chapterView.Hide();
    }

    private void ReloadChapterSelectionPage()
    {
        LoadChapterPageData();
        this.chapterView.ReloadChapterPage();
        JumpToCurrentPage();
    }

    private void LoadChapterPageData()
    {
        int currentLevel = this.runtimeState.CurrentLevelOrder.Value;
        this.chapterView.LoadPageData(this.listChapter, currentLevel);
    }

    private void JumpToCurrentPage()
    {
        int currentChapterIndex = GetCurrentChapterIndex();
        int logicalPage = (int)Mathf.Ceil((float)(currentChapterIndex + 1) / this.chapterView.TotalChapterInAPage) - 1;
        this.currentPageOrder = (this.chapterView.TotalPage - 1) - logicalPage;
        this.chapterView.JumpToPage(this.currentPageOrder);
    }

    private int GetCurrentChapterIndex()
    {
        int currentOrder = this.runtimeState.CurrentLevelOrder.Value;
        LevelInfo currentLevel = this.levelOrder.GetByOrder(currentOrder);

        if (currentLevel == null)
        {
            return 0;
        }

        for (int i = 0; i < this.listChapter.Count; i++)
        {
            ChapterInfo chapter = this.listChapter[i];
            int firstStage = chapter.Levels[0].StageNumber;
            int lastStage = chapter.Levels[chapter.LevelCount - 1].StageNumber;

            if (firstStage <= currentLevel.StageNumber && currentLevel.StageNumber <= lastStage)
            {
                return i;
            }
        }

        return 0;
    }

    private void JumpToFirstPage()
    {
        this.chapterView.JumpToPage(0, EnhancedScroller.TweenType.easeInQuad, 0.5f);
    }

    private void JumpToLastPage()
    {
        this.chapterView.JumpToPage(this.chapterView.TotalPage - 1, EnhancedScroller.TweenType.easeInQuad, 0.5f);
    }

    private void OnChapterCellSelected(ChapterCellView chapterCellView)
    {
        int chapterNumber = chapterCellView.GetChapterOrder();
        ChapterInfo chapter = this.chapterLevelRepo.GetChapter(chapterNumber);

        if (chapter == null)
        {
            return;
        }

        int targetLevelOrder = GetTargetLevelOrder(chapter);
        this.eventPublisher.PublishAsync(new SelectLevel(targetLevelOrder));
        Hide();
    }

    private int GetTargetLevelOrder(ChapterInfo chapter)
    {
        int currentOrder = this.runtimeState.CurrentLevelOrder.Value;
        int firstStage = chapter.Levels[0].StageNumber;
        int lastStage = chapter.Levels[chapter.LevelCount - 1].StageNumber;

        if (firstStage <= (currentOrder + 1) && (currentOrder + 1) <= lastStage)
        {
            // Current chapter: resume at current level (already 0-based)
            return currentOrder;
        }

        // Completed or new chapter: play first level (convert to 0-based)
        return firstStage - 1;
    }

    private void OnClickSettingHandler()
    {
        PlayClickSound();
        this.ScenePresenter.GetViewPresenter<SettingViewPresenter>().Show();
    }

    private void PlayClickSound()
    {
        this.audioService.PlaySound("SFX_Click");
    }

    protected override void AddChildren()
    {
    }
}
