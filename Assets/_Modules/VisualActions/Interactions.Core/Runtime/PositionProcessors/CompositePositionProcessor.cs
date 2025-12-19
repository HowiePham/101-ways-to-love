using UnityEngine;

namespace Mimi.Interactions
{
    public class CompositePositionProcessor : BasePositionProcessor
    {
        [SerializeField] private BasePositionProcessor[] processors;

        public override void Initialize(Transform targetTransform)
        {
            foreach (BasePositionProcessor processor in this.processors)
            {
                processor.Initialize(targetTransform);
            }
        }

        public override Vector3 Process(Vector3 targetPosition)
        {
            Vector3 position = targetPosition;
            foreach (BasePositionProcessor processor in this.processors)
            {
                position = processor.Process(position);
            }
            return position;
        }
    }
}