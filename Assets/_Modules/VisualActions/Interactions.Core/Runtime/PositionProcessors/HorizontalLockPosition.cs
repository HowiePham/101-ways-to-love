using UnityEngine;

namespace Mimi.Interactions
{
    public class HorizontalLockPosition : BasePositionProcessor
    {
        private float startingX;
        public override void Initialize(Transform targetTransform)
        {
            this.startingX = targetTransform.position.x;
        }

        public override Vector3 Process(Vector3 targetPosition)
        {
            targetPosition.x = this.startingX;
            return targetPosition;
        }
    }
}