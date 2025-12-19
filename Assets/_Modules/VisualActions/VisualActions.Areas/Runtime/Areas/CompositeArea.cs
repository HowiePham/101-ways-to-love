using System.Linq;
using UnityEngine;

namespace VisualActions.Areas
{
    public class CompositeArea : BaseArea
    {
        [SerializeField] private BaseArea[] areas;

        public override Vector3 Center
        {
            get
            {
                Vector3 center = Vector3.zero;
                foreach (BaseArea area in this.areas)
                {
                    center += area.Center;
                }

                center /= this.areas.Length;
                return center;
            }
        }

        public override Collider2D Collider => this.areas[0].Collider;

        public override bool Intersect(BaseArea otherArea)
        {
            foreach (BaseArea area in this.areas)
            {
                if (area.Intersect(otherArea)) return true;
            }

            return false;
        }

        public override bool ContainsWorldSpace(Vector2 position)
        {
            return this.areas.Where(area => area.Active).Any(area => area.ContainsWorldSpace(position));
        }

        public override bool ContainsScreenPosition(Vector2 screenPos, Camera renderCamera)
        {
            foreach (BaseArea area in this.areas)
            {
                if (!area.ContainsScreenPosition(screenPos, renderCamera)) return false;
            }

            return true;
        }

        public override void Occupy(Vector2 position)
        {
            foreach (BaseArea area in this.areas)
            {
                if (!area.ContainsWorldSpace(position)) continue;
                area.Occupy(position);
            }
        }
    }
}