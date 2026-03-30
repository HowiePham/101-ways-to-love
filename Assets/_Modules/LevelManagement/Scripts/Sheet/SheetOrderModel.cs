using Mimi.DataSources.GoogleSheet;

namespace Mimi.Prototypes.LevelManagement
{
    [SheetModel]
    public class SheetOrderModel
    {
        public int Order { private set; get; }
        public string Id { private set; get; }
        public string Chapter { private set; get; }
    }
}