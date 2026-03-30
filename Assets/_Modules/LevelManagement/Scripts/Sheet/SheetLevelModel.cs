using Mimi.DataSources.GoogleSheet;

namespace Games
{
    [SheetModel]
    public class SheetLevelModel
    {
        public string Id { private set; get; }
        public string PrefabAddress { private set; get; }
    }
}