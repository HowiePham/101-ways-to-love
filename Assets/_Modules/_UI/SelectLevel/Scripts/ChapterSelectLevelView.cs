using System;
using System.Collections.Generic;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.UI;

public class ChapterSelectLevelView : BaseView, IEnhancedScrollerDelegate
{
    [SerializeField] private Image bgFade;
    [Header("Button")]
    [SerializeField] private Button settingButton;
    [SerializeField] private Button goToTopButton;
    [SerializeField] private Button goToBottomButton;

    [Header("Enhance scroller")]
    [SerializeField] private float cellViewSize;
    [SerializeField] private EnhancedScroller scroller;
    [SerializeField] private ChapterPageCell cellViewPrefab;

    private int currentLevelOrder;
    private SmallList<ChapterInfo> chapterData = new SmallList<ChapterInfo>();

    public int TotalChapterInAPage => this.cellViewPrefab.TotalChapterInAPage;
    public int TotalPage { private set; get; }

    public event Action OnClickSetting;
    public event Action OnTopButtonClick;
    public event Action OnBottomButtonClick;

    public override void Initialize()
    {
        base.Initialize();
        this.scroller.Delegate = this;
        this.settingButton.onClick.AddListener(() => OnClickSetting?.Invoke());
        this.goToTopButton.onClick.AddListener(() => OnTopButtonClick?.Invoke());
        this.goToBottomButton.onClick.AddListener(() => OnBottomButtonClick?.Invoke());
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

    public void LoadPageData(List<ChapterInfo> chapters, int currentLevelOrder)
    {
        this.currentLevelOrder = currentLevelOrder;
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
        var pageCellView = scroller.GetCellView(this.cellViewPrefab) as ChapterPageCell;
        pageCellView.SetData(reversedPageOrder, this.currentLevelOrder, this.chapterData);

        return pageCellView;
    }
}
