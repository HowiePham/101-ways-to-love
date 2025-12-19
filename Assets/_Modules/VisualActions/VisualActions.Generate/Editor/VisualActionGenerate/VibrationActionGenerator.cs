using Mimi.VisualActions.Shaking;

namespace Mimi.VisualActions.Generate.Editor
{
    public class VibrationActionGenerator : BaseGenerateObject<VisualVibrationAction>, IVisualActionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/VibrationAction.prefab";

        public VisualAction Generate()
        {
            return GeneratePrefab();
        }
    }
}