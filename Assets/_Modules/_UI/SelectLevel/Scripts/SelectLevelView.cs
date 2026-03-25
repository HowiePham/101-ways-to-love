using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using Games;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.UI;

public class SelectLevelView : BaseView, IEnhancedScrollerDelegate
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image bgFade;
    [Header("Button")] [SerializeField] private Button removeAdsButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button goToTopButton;
    [SerializeField] private Button goToBottomButton;

    [Header("Enhance scroller")] [SerializeField]
    private float cellViewSize;

    [SerializeField] private EnhancedScroller scroller;
    [SerializeField] private PageCell cellViewPrefab;
    private ILevelRepository levelRepository;
    private int levelTop;
    private float currentStar;

    private SmallList<LevelInfo> levelData = new SmallList<LevelInfo>();

    public int TotalLevelInAPage => this.cellViewPrefab.TotalLevelInAPage;
    public int TotalPage { private set; get; }

    public event Action OnClickRemoveAds;
    public event Action OnClickSetting;
    public event Action OnTopButtonClick;
    public event Action OnBottomButtonClick;

    public override void Initialize()
    {
        base.Initialize();
        this.scroller.Delegate = this;
        this.removeAdsButton.onClick.AddListener(() => OnClickRemoveAds?.Invoke());
        this.settingButton.onClick.AddListener(() => OnClickSetting?.Invoke());
        this.goToTopButton.onClick.AddListener(() => OnTopButtonClick?.Invoke());
        this.goToBottomButton.onClick.AddListener(() => OnBottomButtonClick?.Invoke());
    }

    public override void Show()
    {
        base.Show();
        this.canvasGroup.alpha = 1f;
        this.canvasGroup.interactable = true;
        // FadeIn();
    }

    public override async void Hide()
    {
        // await FadeOut();
        base.Hide();
        this.canvasGroup.alpha = 0f;
        this.canvasGroup.interactable = false;
        this.scroller.scrollerSnapped = null;
        this.scroller.scrollerScrolled = null;
    }

    public void JumpToPage(int pageIndex, EnhancedScroller.TweenType tweenType = EnhancedScroller.TweenType.immediate, float tweenTime = 0)
    {
        this.scroller.JumpToDataIndex(pageIndex, 0, 0, true, tweenType, tweenTime);
        this.scroller.Snap();
    }

    public void ReloadLevelPage()
    {
        this.scroller.ReloadData();
    }

    public void LoadPageData(List<LevelInfo> levelRuntimes, int levelTop)
    {
        this.levelTop = levelTop;

        foreach (var data in levelRuntimes)
        {
            if (this.levelData.Count >= levelRuntimes.Count)
            {
                return;
            }

            this.levelData.Add(data);
        }

        this.TotalPage = (int)Mathf.Ceil((float)this.levelData.Count / TotalLevelInAPage);
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return this.TotalPage;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return this.cellViewSize;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var pageCellView = scroller.GetCellView(this.cellViewPrefab) as PageCell;
        pageCellView.SetData(dataIndex, this.levelTop, this.levelData);

        return pageCellView;
    }

    private void FadeIn()
    {
        this.bgFade.DOFade(0.8f, 0);
        this.bgFade.DOFade(0f, 0.2f).SetEase(Ease.Linear);
    }

    private async Task FadeOut()
    {
        this.bgFade.DOFade(0f, 0);
        this.bgFade.DOFade(0.8f, 0.2f).SetEase(Ease.Linear);
        await UniTask.Delay(200);
    }

    public void SetActiveRemoveAds(bool enable)
    {
        GameObject buttonObj = this.removeAdsButton.gameObject;
        buttonObj.SetActive(enable);
    }
}