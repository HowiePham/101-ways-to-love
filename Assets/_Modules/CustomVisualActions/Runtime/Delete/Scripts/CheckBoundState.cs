using UnityEngine;

namespace VisualFlow
{
    public class CheckBoundState
    {
        public Bounds Bounds { get; }
        public bool Done;

        public CheckBoundState(Bounds bounds)
        {
            Bounds = bounds;
        }
    }
}