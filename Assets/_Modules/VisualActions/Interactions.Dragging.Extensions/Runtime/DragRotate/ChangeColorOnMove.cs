using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class ChangeColorOnMove : MonoBehaviour
    {
        [SerializeField] private Color startColor;
        [SerializeField] private Color completeColor;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private DragRotateCondition condition;

        private void Update()
        {
            if (condition.Validate())
            {
                this.spriteRenderer.color = this.completeColor;
            }
            else
            {
                this.spriteRenderer.color = this.startColor;
            }
        }

      
    }
}