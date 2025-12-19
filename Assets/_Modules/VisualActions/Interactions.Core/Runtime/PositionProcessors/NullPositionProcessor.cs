using UnityEngine;

namespace Mimi.Interactions
{
    public class NullPositionProcessor : BasePositionProcessor
    {
        public override void Initialize(Transform targetTransform)
        {
        }

        public override Vector3 Process(Vector3 targetPosition)
        {
            return targetPosition;
        }
    }
}