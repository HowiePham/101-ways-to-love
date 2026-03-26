using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using Mimi.Prototypes.LevelManagement;
using UnityEngine;

public class ChapterPageCell : EnhancedScrollerCellView
{
    public int TotalChapterInAPage = 9;

    private ChapterCellView[] cells;
    private SmallList<ChapterInfo> chapterData = new SmallList<ChapterInfo>();
    private int currentLevelOrder;

    private void OnEnable()
    {
        this.cells = GetComponentsInChildren<ChapterCellView>();
    }

    public void SetData(int pageOrder, int currentLevelOrder, SmallList<ChapterInfo> chapters)
    {
        this.currentLevelOrder = currentLevelOrder;
        this.chapterData = chapters;
        LoadCellData(pageOrder);
    }

    private void LoadCellData(int pageOrder)
    {
        int firstChapterInPage = pageOrder * this.TotalChapterInAPage;

        for (var cellId = 0; cellId < this.TotalChapterInAPage; cellId++)
        {
            int chapterIndex = firstChapterInPage + cellId;
            ChapterInfo cellData = this.chapterData[chapterIndex];

            if (cellData != null)
            {
                CellStatus cellStatus = SetCellStatus(cellData);
                this.cells[cellId].SetChapterOrderText(cellData.ChapterNumber);
                this.cells[cellId].SetData(cellData.ChapterNumber, cellStatus);
            }
            else
            {
                this.cells[cellId].SetChapterOrderText(0);
                this.cells[cellId].SetData(0, CellStatus.PlainCell);
            }
        }
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
