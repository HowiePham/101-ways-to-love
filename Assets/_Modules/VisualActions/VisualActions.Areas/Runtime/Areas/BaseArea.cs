using UnityEngine;

namespace VisualActions.Areas
{
    public abstract class BaseArea : MonoBehaviour
    {
        public bool Active => gameObject.activeSelf;
        public abstract Vector3 Center { get; }
        public abstract Collider2D Collider { get; }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public abstract bool Intersect(BaseArea otherArea);
        public abstract bool ContainsWorldSpace(Vector2 position);

        public abstract bool ContainsScreenPosition(Vector2 screenPos, Camera renderCamera);

        public abstract void Occupy(Vector2 position);
    }
}