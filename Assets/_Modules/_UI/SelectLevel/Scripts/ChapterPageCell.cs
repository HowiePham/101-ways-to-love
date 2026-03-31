using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using Mimi.Prototypes.LevelManagement;
using UnityEngine;
using UnityEngine.UI;

public class ChapterPageCell : EnhancedScrollerCellView
{
    public int TotalChapterInAPage = 9;

    [SerializeField] private RectMask2D containerMask;
    [SerializeField] private float topPaddingWhenNextPagePlaying;
    [SerializeField] private float topPaddingDefault;

    private ChapterCellView[] cells;
    private SmallList<ChapterInfo> chapterData = new SmallList<ChapterInfo>();
    private int currentLevelOrder;

    private void OnEnable()
    {
        this.cells = GetComponentsInChildren<ChapterCellView>();
    }

    public void SetData(int pageOrder, int currentLevelOrder, SmallList<ChapterInfo> chapters, bool isMaxLevel, bool isFirstChapterOfNextPagePlaying = false)
    {
        this.currentLevelOrder = currentLevelOrder;
        this.chapterData = chapters;
        LoadCellData(pageOrder, isMaxLevel);
        UpdateContainerTopPadding(isFirstChapterOfNextPagePlaying);
    }

    private void UpdateContainerTopPadding(bool isNextPageFirstChapterPlaying)
    {
        if (this.containerMask == null) return;
        float topPadding = isNextPageFirstChapterPlaying ? this.topPaddingWhenNextPagePlaying : this.topPaddingDefault;
        Vector4 padding = this.containerMask.padding;
        padding.w = topPadding;
        this.containerMask.padding = padding;
    }

    private void LoadCellData(int pageOrder, bool isMaxLevel)
    {
        int firstChapterInPage = pageOrder * this.TotalChapterInAPage;

        for (var cellId = 0; cellId < this.TotalChapterInAPage; cellId++)
        {
            int chapterIndex = firstChapterInPage + cellId;
            ChapterInfo cellData = this.chapterData[chapterIndex];

            if (cellData != null)
            {
                CellStatus cellStatus = SetCellStatus(cellData);
                float progress = CalculateProgress(cellData, cellStatus, isMaxLevel);
                this.cells[cellId].SetData(cellData.ChapterNumber, cellData.ChapterIconAddress, cellStatus, progress);
                this.cells[cellId].SetChapterOrderText(cellData.ChapterNumber, cellData.ChapterName);
            }
            else
            {
                this.cells[cellId].SetData(0, string.Empty, CellStatus.PlainCell);
                this.cells[cellId].SetChapterOrderText(0, string.Empty);
            }
        }
    }

    private float CalculateProgress(ChapterInfo chapter, CellStatus status, bool isMaxLevel)
    {
        if (status == CellStatus.Complete || isMaxLevel)
        {
            return 1f;
        }

        if (status == CellStatus.Lock)
        {
            return 0f;
        }

        // Playing: calculate how many levels completed within this chapter
        int firstStage = chapter.Levels[0].StageNumber;
        int currentStage = this.currentLevelOrder + 1;
        int completedInChapter = currentStage - firstStage;
        Debug.Log($"--- (CHAPTER) Chapter {chapter.ChapterNumber} --- {currentStage}/{firstStage} ---> {completedInChapter}");
        return (float)completedInChapter / chapter.LevelCount;
    }

    private CellStatus SetCellStatus(ChapterInfo chapter)
    {
        int firstStage = chapter.Levels[0].StageNumber;
        int lastStage = chapter.Levels[chapter.LevelCount - 1].StageNumber;

        // currentLevelOrder is 0-based, StageNumber is 1-based
        int currentStage = this.currentLevelOrder + 1;

        if (firstStage <= currentStage && currentStage <= lastStage)
        {
            return CellStatus.Playing;
        }

        if (lastStage < currentStage)
        {
            return CellStatus.Complete;
        }

        return CellStatus.Lock;
    }
}