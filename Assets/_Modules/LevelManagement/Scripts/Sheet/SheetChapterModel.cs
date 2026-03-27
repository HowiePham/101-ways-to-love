using Mimi.DataSources.GoogleSheet;

namespace Games
{
    [SheetModel]
    public class SheetChapterModel
    {
        public string Id { private set; get; }
        public string ChapterIconAddress { private set; get; }
        public string ChapterName { private set; get; }
    }
}