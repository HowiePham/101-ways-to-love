using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Mimi.Ads.Adapters;
using Mimi.Games;
using Mimi.Prototypes;
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
    private readonly DisposableBag disposeBag;
    private readonly ISaveManager saveManager;
    private readonly RuntimeState runtimeState;
    List<LevelInfo> listLevel = new List<LevelInfo>();
    private int currentPageOrder = 1;

    public SelectLevelPresenter(BaseScenePresenter scenePresenter, Transform transform,
        ILevelRepository levelDataRepository,
        IAudioService audioService, IAdAdapter adAdapter, RuntimeState runtimeState)
        : base(scenePresenter, transform)
    {
        this.levelRepository = levelDataRepository;
        this.runtimeState = runtimeState;
        this.audioService = audioService;
        this.adAdapter = adAdapter;

        this.disposeBag = new DisposableBag();

        foreach (LevelInfo levelData in levelDataRepository.GetAll())
        {
            this.listLevel.Add(new LevelInfo(levelData.Id, levelData.PrefabAddress, levelData.StageNumber, levelData.IconName));
        }
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
        ReloadLevelSelectionPage();
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