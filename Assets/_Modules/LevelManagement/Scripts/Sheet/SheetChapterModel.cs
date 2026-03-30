using Mimi.DataSources.GoogleSheet;
using Mimi.Prototypes.LevelManagement;

namespace Games
{
    [SheetModel]
    public class SheetChapterModel : IChapterModel
    {
        public string Id { private set; get; }
        public string ChapterIconAddress { private set; get; }
        public string ChapterName { private set; get; }
    }
}