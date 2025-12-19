using Mimi.Actor.Graphic.Core;
using UnityEngine;

namespace ActorServices.Graphic.Draggable.Runtime
{
    public class MonoDraggableItemGraphic : BaseMonoGraphic
    {
        [SerializeField] private DraggableItemField draggableItemField;
        protected override IGraphic WrapperGraphic => CreateGraphic();

        protected override IGraphic CreateGraphic()
        {
            return draggableItemField.GetValue().Graphic;
        }
    }
}