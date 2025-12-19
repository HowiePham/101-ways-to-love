using VisualActions.VisualTransform;

namespace Mimi.VisualActions.Generate.Editor.TransformActions
{
    public class MoveDraggableFromToGenerator : BaseGenerateObject<MoveDraggableFromTo>, IVisualActionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/Transform/MoveDraggableFromTo.prefab";
        public VisualAction Generate()
        {
            return GeneratePrefab();
        }
    }
}