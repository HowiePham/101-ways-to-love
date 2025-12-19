using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Mimi.Debugging.UnityGizmos;
#endif

namespace VisualActions.Areas
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BoxArea : BaseArea
    {
        [SerializeField, OnValueChanged("OnSizeChanged")]
        private Vector2 size = Vector2.one;

        [SerializeField] private BoxCollider2D boxCollider;

        public override Vector3 Center => transform.position;
        public override Collider2D Collider => this.boxCollider;

        public Vector3 TopLeft => Center + new Vector3(-size.x / 2, size.y / 2);
        public Vector3 BottomRight => Center + new Vector3(size.x / 2, -size.y / 2);
        private Transform trans;

        private void Awake()
        {
            this.trans = transform;
            if (this.boxCollider == null)
            {
                this.boxCollider = gameObject.AddComponent<BoxCollider2D>();
                this.boxCollider.isTrigger = true;
                this.boxCollider.size = this.size;
            }
        }

        public void SetSize(Vector2 size)
        {
            this.size = size;
        }

        
        public override bool Intersect(BaseArea otherArea)
        {
            return this.boxCollider.IsTouching(otherArea.Collider);
        }

        public override bool ContainsWorldSpace(Vector2 position)
        {
            return this.boxCollider.bounds.Contains(position);
        }

        public override bool ContainsScreenPosition(Vector2 screenPos, Camera renderCamera)
        {
            var bounds = new Bounds(this.trans.position, this.size);
            return bounds.IntersectRay(renderCamera.ScreenPointToRay(screenPos));
        }

        public override void Occupy(Vector2 position)
        {
            SetActive(false);
        }

#if UNITY_EDITOR
        private Bounds debugBounds;

        private void OnSizeChanged()
        {
            this.boxCollider.size = this.size;
        }

        public void SetSizeEditor(Vector2 size)
        {
            this.size = size;
            OnSizeChanged();
        }
       
        private void OnDrawGizmos()
        {
            this.debugBounds.center = transform.position;
            this.debugBounds.size = this.size;
            VisualDebugger.DrawBounds(this.debugBounds, Color.red);
        }

        private void OnValidate()
        {
            if (this.boxCollider != null)
            {
                this.boxCollider.isTrigger = true;
            }
        }
#endif
    }
}