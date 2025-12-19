using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Data;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class AutoOffsetWhenSelectExtension : MonoDraggableExtension
    {
        [SerializeField] private Vector3Field outputField;
        public override void StartDrag()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var offset = worldPos - BaseDraggable.Position;
            offset.z = 0f;
            outputField.SetValue(-offset);
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            outputField.SetValue(Vector3.zero);
        }
    }
}