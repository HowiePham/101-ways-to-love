using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class DragRotateConditionGenerator : BaseGenerateObject<DragRotateCondition>, IVisualConditionGenerator
    {
        private DragRotateExtenstion dragRotateExtenstion;
        public override string PrefabAddress => "/Prefabs/VisualActions/Conditions/DragRotateCondition.prefab";

        public DragRotateConditionGenerator(DragRotateExtenstion dragRotateExtenstion)
        {
            this.dragRotateExtenstion = dragRotateExtenstion;
        }
        public VisualCondition Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("dragRotateExtenstion", dragRotateExtenstion, AccessModifier.Private);
            return instance;
        }
    }
}