using Mimi.DataSources.GoogleSheet;
using UnityEngine.Scripting;

namespace _Modules.LevelManagement.Scripts.Sheet
{
    [Preserve]
    [SheetModel]
    public class SheetAngelUpgradeModel
    {
        public int Order { private set; get; }
        public MultiParam[] Params { private set; get; }
    }
}