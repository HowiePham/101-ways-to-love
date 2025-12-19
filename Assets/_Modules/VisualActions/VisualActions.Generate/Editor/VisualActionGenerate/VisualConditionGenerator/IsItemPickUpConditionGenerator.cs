using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class IsItemPickUpConditionGenerator : BaseGenerateObject<IsItemPickUp>, IVisualConditionGenerator
    {
        private DraggableItem draggableItem;
        public override string PrefabAddress => "/Prefabs/VisualActions/Conditions/IsPickUpItem.prefab";

        public IsItemPickUpConditionGenerator(DraggableItem draggableItem)
        {
            this.draggableItem = draggableItem;
        }
        public VisualCondition Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("draggableItem", draggableItem, AccessModifier.Private);
            return instance;
        }
    }
}