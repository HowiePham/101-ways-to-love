using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Dragging;

namespace Mimi.VisualActions.Generate.Editor
{
    public class VisualDragGenerator : BaseGenerateObject<VisualDrag>, IVisualActionGenerator
    {
        private IVisualDragExtensionGenerator visualDragExtensionGenerator;
        private IVisualConditionGenerator visualConditionGenerator;
        private bool isCheckCompleteWhenDrag;
        public override string PrefabAddress => "/Prefabs/VisualActions/VisualDrag/VisualDrag.prefab";

        public VisualDragGenerator(IVisualDragExtensionGenerator visualDragExtensionGenerator,
            IVisualConditionGenerator visualConditionGenerator, bool isCheckCompleteWhenDrag = false)
        {
            this.visualDragExtensionGenerator = visualDragExtensionGenerator;
            this.visualConditionGenerator = visualConditionGenerator;
            this.isCheckCompleteWhenDrag = isCheckCompleteWhenDrag;
        }
        public VisualAction Generate()
        {
            var instance = GeneratePrefab();
            VisualCondition condition = null;
            MonoDragExtension dragExtension = null;

            if (visualDragExtensionGenerator != null)
            {
                dragExtension = visualDragExtensionGenerator.Generate();
                dragExtension.transform.SetParent(instance.transform);
            }

            if (visualConditionGenerator != null)
            {
                condition = visualConditionGenerator.Generate();
                condition.transform.SetParent(instance.transform);
            }
            instance.SetField("completeCondition", condition, AccessModifier.Private);
            instance.SetField("dragExtension", dragExtension, AccessModifier.Private);
            instance.SetField("isCheckCompleteWhenDrag", isCheckCompleteWhenDrag, AccessModifier.Private);
            return instance;
        }
        
    }
}