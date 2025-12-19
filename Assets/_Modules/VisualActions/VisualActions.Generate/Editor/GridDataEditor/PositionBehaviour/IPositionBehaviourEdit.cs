using System;
using Mimi.Interactions;
using Mimi.Interactions.Dragging;
using Mimi.VisualActions.Generate.Editor;

namespace Mimi.VisualActions.Generate.Editor
{
    using System;

    public interface IPositionBehaviourEdit
    {
        public void SetDraggable(BaseDraggable draggable);
    }

    public interface IPositionBehaviourEdit<T> : IPositionBehaviourEdit, IDataEdit
        where T : BasePositionProcessor
    {
        T Processor { get; }
        bool IsToggle { get; set; }
        void TurnOnHandle();
        void TurnOffHandle();
    }
}