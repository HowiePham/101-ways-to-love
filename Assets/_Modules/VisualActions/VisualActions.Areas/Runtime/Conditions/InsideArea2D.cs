using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Dragging
{
    public class InsideArea2D : VisualCondition
    {
        [SerializeField] protected Transform checkTransform;
        [SerializeField] protected BaseArea targetArea;
        public Transform CheckTransform => this.checkTransform;
        public BaseArea TargetArea => this.targetArea;

        public override bool Validate()
        {
            if (!this.TargetArea.Active || !this.checkTransform.gameObject.activeSelf) return false;
            Vector3 checkPos = this.CheckTransform.position;
            return this.TargetArea.ContainsWorldSpace(checkPos);
        }
    }
}