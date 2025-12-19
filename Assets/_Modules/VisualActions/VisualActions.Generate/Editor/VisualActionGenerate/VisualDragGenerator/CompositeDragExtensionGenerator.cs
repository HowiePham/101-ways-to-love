using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class CompositeDragExtensionGenerator : BaseCompositeGenerateObject<CompositeDragExtension, IVisualDragExtensionGenerator>, IVisualDragExtensionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/VisualDrag/Extensions/CompositeDragExtension.prefab";
        public MonoDragExtension Generate()
        {
            var instance = GeneratePrefab();
            var extensions = new MonoDragExtension[generators.Count];
            for (int i = 0; i < generators.Count; i++)
            {
                extensions[i] = generators[i].Generate();
                extensions[i].transform.SetParent(instance.transform);
            }
            instance.SetField("extensions", extensions, AccessModifier.Private);
            return instance;
        }
    }
}