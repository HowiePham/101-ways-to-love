using System.Collections.Generic;
using _Modules.GameEvent.Scripts;
using _Modules.Gameflow_Events_.Scripts;
using EnhancedUI.EnhancedScroller;
using Mimi.Ads.Adapters;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.SaveLoad;
using Mimi.Prototypes.UI;
using UnityEngine;

public class SelectLevelPresenter : BaseViewPresenter
{
    private SelectLevelView selectLevelView;

    private readonly ILevelRepository levelRepository;
    private readonly IAudioService audioService;
    private readonly IAdAdapter adAdapter;
    private readonly IAsyncPublisher eventPublisher;
    private readonly DisposableBag disposeBag;
    private readonly ISaveManager saveManager;
    private readonly RuntimeState runtimeState;
    List<LevelInfo> listLevel = new List<LevelInfo>();
    private int currentPageOrder = 1;

    public SelectLevelPresenter(BaseScenePresenter scenePresenter, Transform transform,
        ILevelRepository levelDataRepository,
        IAudioService audioService, IAdAdapter adAdapter, RuntimeState runtimeState,
        IAsyncPublisher eventPublisher)
        : base(scenePresenter, transform)
    {
        this.levelRepository = levelDataRepository;
        this.runtimeState = runtimeState;
        this.audioService = audioService;
        this.adAdapter = adAdapter;
        this.eventPublisher = eventPublisher;

        this.disposeBag = new DisposableBag();

        foreach (LevelInfo levelData in levelDataRepository.GetAll())
        {
            this.listLevel.Add(new LevelInfo(levelData.Id, levelData.PrefabAddress,
                levelData.StageNumber, levelData.Chapter, levelData.IconName));
        }
    }

    protected override void AddViews()
    {
        this.selectLevelView = AddView<SelectLevelView>();
    }

    protected override void OnShow()
    {
        base.OnShow();
        this.eventPublisher.PublishAsync(new ScreenShown("select_level"));

        this.selectLevelView.OnClickSetting += OnClickSettingHandler;
        this.selectLevelView.OnTopButtonClick += JumpToFirstPage;
        this.selectLevelView.OnBottomButtonClick += JumpToLastPage;

        Messenger.AddListener<LevelCellView>(EventKey.SelectLevel, OnLevelCellSelected);

        this.adAdapter.Mrec.Hide();
        ReloadLevelSelectionPage();
    }


    private void JumpToCurrentPage()
    {
        int logicalPage = (int)Mathf.Ceil((float)this.runtimeState.TopLevelOrder.Value / this.selectLevelView.TotalLevelInAPage) - 1;
        this.currentPageOrder = (this.selectLevelView.TotalPage - 1) - logicalPage;
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
        int levelTop = this.runtimeState.TopLevelOrder.Value;

        this.selectLevelView.LoadPageData(this.listLevel, levelTop);
    }

    private void OnClickSettingHandler()
    {
        PlayClickSound();
        var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
        settingViewPresenter.Show();
        settingViewPresenter.SetActiveHomeButton(false);
        settingViewPresenter.SetActiveReplayButton(false);
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

        Messenger.RemoveListener<LevelCellView>(EventKey.SelectLevel, OnLevelCellSelected);

        this.disposeBag.Dispose();
        this.selectLevelView.Hide();
    }

    private void OnLevelCellSelected(LevelCellView levelCellView)
    {
        int levelOrder = levelCellView.GetLevelOrder() - 1;
        this.eventPublisher.PublishAsync(new SelectLevel(levelOrder));
        Hide();
    }

    private void PlayClickSound()
    {
        this.audioService.PlaySound("SFX_Click");
    }

    protected override void AddChildren()
    {
    }
}