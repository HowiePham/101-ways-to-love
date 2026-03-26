using EnhancedUI;
using Mimi.Prototypes.LevelManagement;
using UnityEngine;

public class LevelPageCell : PageCell
{
    private LevelCellView[] cells;
    private int totalPages = 1;
    private SmallList<LevelInfo> levelData = new SmallList<LevelInfo>();
    private int levelTop;

    private void OnEnable()
    {
        this.cells = GetComponentsInChildren<LevelCellView>();
    }

    public override void SetData(int pageOrder, int levelTop, SmallList<LevelInfo> levels)
    {
        this.levelTop = levelTop;
        this.levelData = levels;
        LoadCellData(pageOrder);
    }

    private void LoadCellData(int pageOrder)
    {
        int firstLevelInPage = pageOrder * this.TotalLevelInAPage;
        for (var cellId = 0; cellId < this.TotalLevelInAPage; cellId++)
        {
            int levelOrder = firstLevelInPage + cellId;
            LevelInfo cellData = this.levelData[levelOrder];

            this.cells[cellId].SetLevelOrderText(levelOrder);
            if (cellData != null)
            {
                int cellOrder = cellData.StageNumber;
                CellStatus cellStatus = SetCellStatus(cellOrder);
                Debug.Log($"--- (LEVEL CELL) Set LVOrder: {cellOrder}/{this.levelTop} --- Status: {cellStatus}");

                this.cells[cellId].SetData(cellOrder, cellData.IconName, cellStatus);
            }
            else
            {
                this.cells[cellId].SetData(1, "Icons/icon_level-001", CellStatus.PlainCell);
            }
        }
    }

    public override void SetPageType(PageType data)
    {
        base.SetPageType(PageType.Normal);
    }

    private CellStatus SetCellStatus(int dataIndex)
    {
        if (dataIndex == this.levelTop)
        {
            return CellStatus.Playing;
        }

        if (dataIndex < this.levelTop)
        {
            return CellStatus.Complete;
        }

        return CellStatus.Lock;
    }
}