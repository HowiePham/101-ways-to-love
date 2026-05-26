using Mimi.DataSources.GoogleSheet;

namespace Mimi.Prototypes.LevelManagement
{
    [SheetModel]
    public class SheetAngelSkinModel
    {
        public string Id { private set; get; }
        public string SkinName { private set; get; }
        public string SkinType { private set; get; }
    }
}