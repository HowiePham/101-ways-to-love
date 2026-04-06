using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Data;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class KeepPositionInRange : MonoDraggableExtension
    {
        [SerializeField] private Vector2 xPosition;
        [SerializeField] private Vector2 yPosition;
        [SerializeField] private Vector3Field inputField;

        public override void StartDrag()
        {
        }

        public override void Drag()
        {
            Vector3 currentPos = this.BaseDraggable.transform.position;
            Vector3 posAfterAddInputField = currentPos + this.inputField.GetValue();
            if (posAfterAddInputField.x <= this.xPosition.x)
            {
                currentPos.x = this.xPosition.x - this.inputField.GetValue().x;
            }
            else if (posAfterAddInputField.x >= this.xPosition.y)
            {
                currentPos.x = this.xPosition.y - this.inputField.GetValue().x;
            }

            if (posAfterAddInputField.y <= this.yPosition.x)
            {
                currentPos.y = this.yPosition.x - this.inputField.GetValue().y;
            }
            else if (posAfterAddInputField.y >= this.yPosition.y)
            {
                currentPos.y = this.yPosition.y - this.inputField.GetValue().y;
            }

            this.BaseDraggable.SetPosition(currentPos);
        }

        public override void EndDrag()
        {
        }
    }
}