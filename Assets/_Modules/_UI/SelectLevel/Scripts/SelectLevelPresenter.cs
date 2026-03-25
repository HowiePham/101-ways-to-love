using System.Collections.Generic;
using Economy.Resources;
using EnhancedUI.EnhancedScroller;
using MEC;
using Mimi.Ads.Adapters;
using Mimi.Configs;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Prototypes;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.SaveLoad;
using Mimi.Prototypes.UI;
using UnityEngine;

public class SelectLevelPresenter : BaseViewPresenter
{
    private SelectLevelView selectLevelView;

    private readonly GameData gameData;
    private readonly DialogManager dialogManager;
    private readonly ILevelRepository levelRepository;
    private readonly IAudioService audioService;
    private readonly IAdAdapter adAdapter;
    private readonly IAsyncPublisher eventPublisher;
    private readonly IAsyncSubscriber eventSubscriber;
    private readonly DisposableBag disposeBag;
    private readonly ISaveManager saveManager;
    private readonly IConfigProvider configProvider;
    private readonly RuntimeState runtimeState;
    private readonly IResourceCollection resourceCollection;
    List<LevelInfo> listLevel = new List<LevelInfo>();
    private int currentPageOrder = 1;
    private bool isShowPlayGap;
    private bool buyAtIAPPopup;
    private CoroutineHandle unlockAllCountDownCoroutine;
    private CoroutineHandle clickingTimeCountDownCoroutine;

    public SelectLevelPresenter(BaseScenePresenter scenePresenter, Transform transform,
        ILevelRepository levelDataRepository, GameData gameData, DialogManager dialogManager,
        IAudioService audioService, IAdAdapter adAdapter,
        IAsyncPublisher eventPublisher, IAsyncSubscriber eventSubscriber, IConfigProvider configProvider,
        IResourceCollection resourceCollection, RuntimeState runtimeState)
        : base(scenePresenter, transform)
    {
        this.gameData = gameData;
        this.dialogManager = dialogManager;
        this.levelRepository = levelDataRepository;
        this.configProvider = configProvider;
        this.resourceCollection = resourceCollection;
        this.runtimeState = runtimeState;
        this.audioService = audioService;
        this.adAdapter = adAdapter;
        this.eventPublisher = eventPublisher;
        this.eventSubscriber = eventSubscriber;

        this.disposeBag = new DisposableBag();

        foreach (LevelInfo levelData in levelDataRepository.GetAll())
        {
            this.listLevel.Add(new LevelInfo(levelData.Id, levelData.PrefabAddress, levelData.StageNumber));
        }
#if PLAYGAP_DEPENDENCIES_INSTALLED
        Messenger.AddListener(EventKey.OnShowClaimBannerPlayGap, SetCanShowPlayGap);
#endif
    }

    protected override void AddViews()
    {
        this.selectLevelView = AddView<SelectLevelView>();
    }

    protected override void OnShow()
    {
        base.OnShow();

        this.selectLevelView.OnClickSetting += OnClickSettingHandler;
        this.selectLevelView.OnTopButtonClick += JumpToFirstPage;
        this.selectLevelView.OnBottomButtonClick += JumpToLastPage;

        this.adAdapter.Mrec.Hide();

#if PLAYGAP_DEPENDENCIES_INSTALLED
        WaitShowPlayGap();
#endif
    }


    private void JumpToCurrentPage()
    {
        this.currentPageOrder = (int)Mathf.Ceil((float)this.runtimeState.CurrentLevelOrder.Value / this.selectLevelView.TotalLevelInAPage) - 1;
        this.selectLevelView.JumpToPage(this.currentPageOrder);
    }

    private void JumpToFirstPage()
    {
        this.selectLevelView.JumpToPage(0, EnhancedScroller.TweenType.easeInQuad, 0.5f);
    }

    private void JumpToLastPage()
    {
        this.selectLevelView.JumpToPage(this.selectLevelView.TotalPage - 1, EnhancedScroller.TweenType.easeInQuad, 0.5f);
    }

    private void LoadLevelPageData()
    {
        int levelTop = this.runtimeState.CurrentLevelOrder.Value;

        this.selectLevelView.LoadPageData(this.listLevel, levelTop);
    }

    private void OnClickSettingHandler()
    {
        PlayClickSound();
        this.ScenePresenter.GetViewPresenter<SettingViewPresenter>().Show();
    }

    private void ReloadLevelSelectionPage()
    {
        LoadLevelPageData();
        this.selectLevelView.ReloadLevelPage();
        JumpToCurrentPage();
    }
    

    protected override void OnHide()
    {
        base.OnHide();
        this.selectLevelView.OnClickSetting -= OnClickSettingHandler;
        this.selectLevelView.OnTopButtonClick -= JumpToFirstPage;
        this.selectLevelView.OnBottomButtonClick -= JumpToLastPage;

        this.disposeBag.Dispose();
        this.selectLevelView.Hide();
    }

    private void PlayClickSound()
    {
        this.audioService.PlaySound("SFX_Click");
    }

    protected override void AddChildren()
    {
    }
}