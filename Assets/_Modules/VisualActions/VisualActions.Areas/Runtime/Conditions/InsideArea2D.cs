using Mimi.VisualActions;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Dragging
{
    public class InsideArea2D : VisualCondition
    {
        [SerializeField] private Transform checkTransform;
        [SerializeField] private BaseArea targetArea;

        public override bool Validate()
        {
            if (!this.targetArea.Active) return false;
            Vector3 checkPos = this.checkTransform.position;
            //Debug.Log("hello "+this.targetArea.ContainsWorldSpace(checkPos));
            return this.targetArea.ContainsWorldSpace(checkPos);
        }
    }
}