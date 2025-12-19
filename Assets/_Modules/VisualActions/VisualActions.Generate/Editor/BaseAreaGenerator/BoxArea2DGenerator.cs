using VisualActions.Areas;

namespace Mimi.VisualActions.Generate.Editor
{
    public class BoxArea2DGenerator : BaseGenerateObject<BoxArea>, IBaseAreaGenerator
    {
        public override string PrefabAddress => "/Prefabs/Areas/BoxArea.prefab";
        public BaseArea Generate()
        {
            return GeneratePrefab();
        }
    }
}