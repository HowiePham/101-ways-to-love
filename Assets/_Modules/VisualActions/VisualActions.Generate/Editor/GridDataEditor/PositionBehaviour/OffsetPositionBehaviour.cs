using Mimi.Interactions;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Data;

namespace Mimi.VisualActions.Generate.Editor
{
    public class OffsetPositionBehaviour : BasePositionBehaviourEdit<OffsetPositionProcessor>
    {
        public override void TurnOnHandle()
        {
            base.TurnOnHandle();
            this.Processor.SetField("inputField", Draggable.GetComponent<Vector3Field>(), AccessModifier.Private);
        }
    }
}