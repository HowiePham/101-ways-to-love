using UnityEngine;

namespace Mimi.Interactions
{
    public class VerticalLockPosition : BasePositionProcessor
    {
        private float startingY;

        public override void Initialize(Transform targetTransform)
        {
            this.startingY = targetTransform.position.y;
        }

        public override Vector3 Process(Vector3 targetPosition)
        {
            targetPosition.y = this.startingY;
            
            return targetPosition;
        }
    }
}