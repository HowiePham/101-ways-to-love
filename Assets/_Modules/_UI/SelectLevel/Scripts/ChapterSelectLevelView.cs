using System;
using System.Collections.Generic;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChapterSelectLevelView : BaseView, IEnhancedScrollerDelegate
{
    [SerializeField] private Image bgFade;
    [Header("Button")] [SerializeField] private Button settingButton;
    [SerializeField] private Button iapButton;
    [SerializeField] private Button goToTopButton;
    [SerializeField] private Button goToBottomButton;

    [Header("Life View")] [SerializeField] private NumberBasedLifeView lifeView;

    [Header("Enhance scroller")] [SerializeField]
    private float cellViewSize;

    [SerializeField] private EnhancedScroller scroller;
    [SerializeField] private ChapterPageCell cellViewPrefabRight;
    [SerializeField] private ChapterPageCell cellViewPrefabLeft;

    private int currentLevelOrder;
    private SmallList<ChapterInfo> chapterData = new SmallList<ChapterInfo>();

    public NumberBasedLifeView LifeView => this.lifeView;
    public int TotalChapterInAPage => this.cellViewPrefabRight.TotalChapterInAPage;
    public int TotalPage { private set; get; }

    public event Action OnClickSetting;
    public event Action OnTopButtonClick;
    public event Action OnBottomButtonClick;
    public event Action OnIAPButtonClick;

    private bool isMaxLevel;

    public override void Initialize()
    {
        base.Initialize();
        if (this.lifeView != null)
        {
            this.lifeView.Initialize();
        }

        this.scroller.Delegate = this;
        this.settingButton.onClick.AddListener(() => OnClickSetting?.Invoke());
        this.goToTopButton.onClick.AddListener(() => OnTopButtonClick?.Invoke());
        this.goToBottomButton.onClick.AddListener(() => OnBottomButtonClick?.Invoke());
        this.iapButton.onClick.AddListener(() => OnIAPButtonClick?.Invoke());
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
        this.scroller.scrollerSnapped = null;
        this.scroller.scrollerScrolled = null;
    }

    public void JumpToPage(int pageIndex, EnhancedScroller.TweenType tweenType = EnhancedScroller.TweenType.immediate, float tweenTime = 0)
    {
        this.scroller.JumpToDataIndex(pageIndex, 0, 0, true, tweenType, tweenTime);
        this.scroller.Snap();
    }

    public void ReloadChapterPage()
    {
        this.scroller.ReloadData();
    }

    public void LoadPageData(List<ChapterInfo> chapters, int currentLevelOrder, bool isMaxLevel)
    {
        this.currentLevelOrder = currentLevelOrder;
        this.isMaxLevel = isMaxLevel;
        this.chapterData.Clear();

        foreach (var chapter in chapters)
        {
            this.chapterData.Add(chapter);
        }

        int totalSlots = TotalChapterInAPage * (int)Mathf.Ceil((float)chapters.Count / TotalChapterInAPage);
        for (int i = chapters.Count; i < totalSlots; i++)
        {
            this.chapterData.Add(null);
        }

        this.TotalPage = (int)Mathf.Ceil((float)chapters.Count / TotalChapterInAPage);
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
        int reversedPageOrder = (this.TotalPage - 1) - dataIndex;
        bool isRightSide = reversedPageOrder % 2 == 0;
        ChapterPageCell pageCellView = isRightSide
            ? scroller.GetCellView(this.cellViewPrefabRight) as ChapterPageCell
            : scroller.GetCellView(this.cellViewPrefabLeft) as ChapterPageCell;
        
        bool isFirstChapterOfNextPagePlaying = false;
        int nextPageOrder = reversedPageOrder + 1;
        if (nextPageOrder < this.TotalPage)
        {
            int firstChapterIndexOfNextPage = nextPageOrder * TotalChapterInAPage;
            ChapterInfo firstChapterOfNextPage = this.chapterData[firstChapterIndexOfNextPage];
            if (firstChapterOfNextPage != null)
            {
                isFirstChapterOfNextPagePlaying = IsChapterPlaying(firstChapterOfNextPage);
            }
        }

        pageCellView.SetData(reversedPageOrder, this.currentLevelOrder, this.chapterData, this.isMaxLevel, isFirstChapterOfNextPagePlaying);
        return pageCellView;
    }

    private bool IsChapterPlaying(ChapterInfo chapter)
    {
        int firstStage = chapter.Levels[0].StageNumber;
        int lastStage = chapter.Levels[chapter.LevelCount - 1].StageNumber;
        int currentStage = this.currentLevelOrder + 1;
        return firstStage <= currentStage && currentStage <= lastStage;
    }

    public void SetActiveRemoveAdsButton(bool active)
    {
        this.iapButton.gameObject.SetActive(active);
    }
}