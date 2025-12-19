using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Data;
using UnityEngine;

namespace Mimi.Interactions
{
    public class OffsetPositionProcessor : BasePositionProcessor
    {
        [HideInBehaviourEditor]
        [SerializeField] private Vector3Field inputField;
        public override void Initialize(Transform targetTransform)
        {
            
        }

        public override Vector3 Process(Vector3 targetPosition)
        {
            return targetPosition + inputField.GetValue();
        }
    }
}