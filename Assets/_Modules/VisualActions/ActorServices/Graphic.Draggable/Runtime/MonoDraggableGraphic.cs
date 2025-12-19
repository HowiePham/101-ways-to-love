using Mimi.Actor.Graphic.Core;
using Mimi.Interactions.Dragging;
using UnityEngine;

namespace ActorServices.Graphic.Draggable.Runtime
{
    public class MonoDraggableGraphic : BaseMonoGraphic
    {
        [SerializeField] private DraggableField draggable;

        protected override IGraphic WrapperGraphic => draggable.GetValue().Graphic;

        protected override IGraphic CreateGraphic()
        {
            return null;
        }
    }
}