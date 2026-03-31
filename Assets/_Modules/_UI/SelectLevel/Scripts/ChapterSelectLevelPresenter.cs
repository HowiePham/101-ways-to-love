using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Modules.Gameflow_Events_.Scripts;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using Games;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Prototypes;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.UI;
using UnityEngine;

public class ChapterSelectLevelPresenter : BaseViewPresenter
{
    private ChapterSelectLevelView chapterView;
    private NumberBasedLifeView lifeView;

    private readonly ChapterLevelRepository chapterLevelRepo;
    private readonly ILevelOrder levelOrder;
    private readonly IAudioService audioService;
    private readonly IAdAdapter adAdapter;
    private readonly IAsyncPublisher eventPublisher;
    private readonly IAsyncSubscriber eventSubscriber;
    private readonly DisposableBag disposeBag;
    private readonly RuntimeState runtimeState;
    private readonly LifeSystem lifeSystem;
    private readonly List<ChapterInfo> listChapter;
    private int currentPageOrder = 1;
    private bool IsMaxLevel => this.levelOrder.IsLast(this.levelOrder.GetByOrder(this.runtimeState.TopLevelOrder.Value).Id);

    public ChapterSelectLevelPresenter(BaseScenePresenter scenePresenter, Transform transform,
        ChapterLevelRepository chapterLevelRepo, ILevelOrder levelOrder,
        IAudioService audioService, IAdAdapter adAdapter, RuntimeState runtimeState,
        IAsyncPublisher eventPublisher, IAsyncSubscriber eventSubscriber,
        LifeSystem lifeSystem)
        : base(scenePresenter, transform)
    {
        this.chapterLevelRepo = chapterLevelRepo;
        this.levelOrder = levelOrder;
        this.runtimeState = runtimeState;
        this.audioService = audioService;
        this.adAdapter = adAdapter;
        this.eventPublisher = eventPublisher;
        this.eventSubscriber = eventSubscriber;
        this.lifeSystem = lifeSystem;
        this.disposeBag = new DisposableBag();

        this.listChapter = new List<ChapterInfo>(this.chapterLevelRepo.GetChapters());
    }

    protected override void AddViews()
    {
        this.chapterView = AddView<ChapterSelectLevelView>();
        this.lifeView = this.chapterView.LifeView;
    }

    protected override void OnShow()
    {
        base.OnShow();

        this.chapterView.OnClickSetting += OnClickSettingHandler;
        this.chapterView.OnTopButtonClick += JumpToFirstPage;
        this.chapterView.OnBottomButtonClick += JumpToLastPage;
        this.chapterView.OnIAPButtonClick += IAPClickHandler;

        var baseGameContext = (BaseGameContext)this.Context;
        this.chapterView.SetActiveRemoveAdsButton(!baseGameContext.IsRemoveAds);

        Messenger.AddListener<ChapterCellView>(EventKey.SelectChapter, OnChapterCellSelected);

        this.lifeView.Show();
        this.lifeView.OnLifeButtonClicked += LifeButtonClickedHandler;
        this.eventSubscriber.Subscribe<LifeUpdated>(OnLifeUpdate).AddToBag(this.disposeBag);
        this.eventSubscriber.Subscribe<RecoveryLifeTimerUpdated>(OnRecoveryTimerUpdate).AddToBag(this.disposeBag);

        this.lifeView.SetLifeCount(this.lifeSystem.CurrentLifeCount);
        this.lifeView.SetTimeRemaining(this.lifeSystem.IsLifeIsFull() ? "FULL" : this.lifeSystem.GetRemainingTime());
        this.lifeView.SetAddLifeIconActive(!this.lifeSystem.IsLifeIsFull());

        this.adAdapter.Mrec.Hide();
        ReloadChapterSelectionPage();
    }

    protected override void OnHide()
    {
        base.OnHide();

        this.chapterView.OnClickSetting -= OnClickSettingHandler;
        this.chapterView.OnTopButtonClick -= JumpToFirstPage;
        this.chapterView.OnBottomButtonClick -= JumpToLastPage;
        this.chapterView.OnIAPButtonClick -= IAPClickHandler;

        Messenger.RemoveListener<ChapterCellView>(EventKey.SelectChapter, OnChapterCellSelected);

        this.lifeView.OnLifeButtonClicked -= LifeButtonClickedHandler;
        this.lifeView.Hide();
        this.disposeBag.Dispose();
    }

    private void IAPClickHandler()
    {
        this.ScenePresenter.GetViewPresenter<RemoveAdsViewPresenter>().Show();
    }

    private void ReloadChapterSelectionPage()
    {
        LoadChapterPageData();
        this.chapterView.ReloadChapterPage();
        JumpToCurrentPage();
    }

    private void LoadChapterPageData()
    {
        int topLevel = this.runtimeState.TopLevelOrder.Value;

        Debug.Log($"--- (CHAPTER) Is Max Level: {this.runtimeState.TopLevelOrder.Value}/{this.levelOrder.GetAllOrdered().ToList().Count} ---> {IsMaxLevel}");
        this.chapterView.LoadPageData(this.listChapter, topLevel, IsMaxLevel);
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
        int currentOrder = this.runtimeState.TopLevelOrder.Value;
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
        int topOrder = this.runtimeState.TopLevelOrder.Value;
        int firstStage = chapter.Levels[0].StageNumber;
        int lastStage = chapter.Levels[chapter.LevelCount - 1].StageNumber;

        if (firstStage <= (topOrder + 1) && (topOrder + 1) <= lastStage)
        {
            // Current chapter: resume at top level (already 0-based)
            return topOrder;
        }

        // Completed or new chapter: play first level (convert to 0-based)
        return firstStage - 1;
    }

    private async UniTask OnLifeUpdate(LifeUpdated lifeUpdated, CancellationToken cancellationToken)
    {
        int currentLifeCount = lifeUpdated.LifeCount;
        this.lifeView.SetLifeCount(currentLifeCount);
        this.lifeView.SetAddLifeIconActive(!this.lifeSystem.IsLifeIsFull());

        if (this.lifeSystem.IsLifeIsFull())
        {
            this.lifeView.SetTimeRemaining("FULL");
        }

        await UniTask.CompletedTask;
    }

    private async UniTask OnRecoveryTimerUpdate(RecoveryLifeTimerUpdated recoveryLifeTimerUpdated, CancellationToken cancellationToken)
    {
        this.lifeView.SetTimeRemaining(this.lifeSystem.IsLifeIsFull() ? "FULL" : recoveryLifeTimerUpdated.RemainingTime);
        await UniTask.CompletedTask;
    }

    private void LifeButtonClickedHandler()
    {
        this.lifeSystem.ShowGetMoreLifeDialog(DialogId.GetMoreLifeDialog);
    }

    private void OnClickSettingHandler()
    {
        var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
        settingViewPresenter.Show();
        settingViewPresenter.SetActiveHomeButton(false);
        settingViewPresenter.SetActiveReplayButton(false);
    }
    
    protected override void AddChildren()
    {
    }
}