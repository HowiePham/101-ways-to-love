using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using Mimi.Prototypes.LevelManagement;

public class PageCell : EnhancedScrollerCellView
{
    public PageType pageType;
    public int TotalLevelInAPage = 15;

    public virtual void SetData(int dataIndex, int levelTop, SmallList<LevelInfo> levelData)
    {
    }

    public virtual void SetPageType(PageType pageType)
    {
        this.pageType = pageType;
    }
}