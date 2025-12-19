using VisualActions.VisualTransform;

namespace Mimi.VisualActions.Generate.Editor.TransformActions
{
    public class MoveDraggableByDistanceGenerator : BaseGenerateObject<MoveDraggableByDistance>, IVisualActionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/Transform/MoveDraggableByDistance.prefab";
        public VisualAction Generate()
        {
            return GeneratePrefab();
        }
    }
}