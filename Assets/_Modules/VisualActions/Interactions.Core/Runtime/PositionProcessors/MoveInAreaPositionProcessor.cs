using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.Interactions
{
    [Serializable]
    public class MoveInAreaPositionProcessor : BasePositionProcessor
    {
        [SerializeField] private BoxArea baseArea;

        public override void Initialize(Transform targetTransform)
        {
            
        }

        public override Vector3 Process(Vector3 targetPosition)
        {
            var topLeft = baseArea.TopLeft;
            var bottomRight = baseArea.BottomRight;
            targetPosition.x = Mathf.Clamp(targetPosition.x, topLeft.x, bottomRight.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, bottomRight.y, topLeft.y);
            return targetPosition;
        }
    }
}