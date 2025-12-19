using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Mimi.Debugging.UnityGizmos;
#endif

namespace VisualActions.Drawing
{
    public class DrawAllBoxValidator : BaseDrawResultValidator
    {
        [SerializeField] private Transform checkBoxRoot;
        [SerializeField] private Vector3 checkBoxSize = new(2f, 2f, 2f);

        private List<CheckBoundState> detectBounds;

        public override void Initialize()
        {
            this.detectBounds = CreateDetectBoxes();
        }

        public override bool Validate(Vector3 pointPosition)
        {
            foreach (CheckBoundState bound in this.detectBounds)
            {
                if (bound.Done) continue;
                bound.Done = bound.Bounds.Contains(pointPosition);
            }

            return CheckComplete();
        }

        private bool CheckComplete()
        {
            foreach (CheckBoundState bound in this.detectBounds)
            {
                if (!bound.Done)
                {
                    return false;
                }
            }

            return true;
        }

        public override void Clear()
        {
            foreach (CheckBoundState bound in this.detectBounds)
            {
                bound.Done = false;
            }
        }

        private List<CheckBoundState> CreateDetectBoxes()
        {
            var bounds = new List<CheckBoundState>();

            for (int i = 0; i < this.checkBoxRoot.childCount; i++)
            {
                Transform pointTrans = this.checkBoxRoot.GetChild(i);
                var bound = new Bounds(pointTrans.position, this.checkBoxSize);
                var boundState = new CheckBoundState(bound);
                this.detectBounds.Add(boundState);
            }

            return bounds;
        }


        private class CheckBoundState
        {
            public Bounds Bounds { get; }
            public bool Done;

            public CheckBoundState(Bounds bounds)
            {
                Bounds = bounds;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for (int i = 0; i < this.checkBoxRoot.childCount; i++)
            {
                Transform pointTrans = this.checkBoxRoot.GetChild(i);
                var bound = new Bounds(pointTrans.position, this.checkBoxSize);
                VisualDebugger.DrawBounds(bound, Color.red);
            }
        }
#endif
    }
}