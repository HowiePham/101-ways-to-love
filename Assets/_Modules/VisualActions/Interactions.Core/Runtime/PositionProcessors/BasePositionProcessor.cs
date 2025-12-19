using UnityEngine;

namespace Mimi.Interactions
{
    public abstract class BasePositionProcessor : MonoBehaviour
    {
        public abstract void Initialize(Transform targetTransform);
        public abstract Vector3 Process(Vector3 targetPosition);
    }
}