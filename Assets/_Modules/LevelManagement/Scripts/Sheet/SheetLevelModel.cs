using Mimi.DataSources.GoogleSheet;

namespace Games
{
    [SheetModel]
    public class SheetLevelModel
    {
        public int StageNumber { private set; get; }
        public int Chapter { private set; get; }
        public string Id { private set; get; }
        public string PrefabAddress { private set; get; }
    }
}