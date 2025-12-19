using UnityEngine;

namespace VisualActions.Areas
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class PolygonArea : BaseArea
    {
        [SerializeField] private PolygonCollider2D polygonCollider;

        public override Vector3 Center => this.polygonCollider.bounds.center;
        public override Collider2D Collider => this.polygonCollider;

        public override bool Intersect(BaseArea otherArea)
        {
            return this.polygonCollider.IsTouching(otherArea.Collider);
        }

        public override bool ContainsWorldSpace(Vector2 position)
        {
            return this.polygonCollider.OverlapPoint(position);
        }

        public override bool ContainsScreenPosition(Vector2 screenPos, Camera renderCamera)
        {
            Vector3 worldPoint = renderCamera.ScreenToWorldPoint(screenPos);
            worldPoint.z = this.polygonCollider.transform.position.z;
            return this.polygonCollider.OverlapPoint(worldPoint);
        }

        public override void Occupy(Vector2 position)
        {
            SetActive(false);
        }
    }
}