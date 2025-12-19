using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Swiping
{
    public enum SwipeDirection
    {
        Up = 0,
        Down = 1,
        Right = 2,
        Left = 3
    }

    public class VisualSwipe : VisualAction
    {
        [SerializeField, Required] private BaseArea startingArea;
        [SerializeField] private SwipeDirection direction;

        private bool complete;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.complete = false;
            LeanTouch.OnFingerSwipe += FingerSwipeHandler;
            await UniTask.WaitUntil(() => this.complete, cancellationToken: cancellationToken);
            LeanTouch.OnFingerSwipe -= FingerSwipeHandler;
        }

        private void FingerSwipeHandler(LeanFinger finger)
        {
            if (!this.startingArea.ContainsScreenPosition(finger.StartScreenPosition, Camera.main)) return;
            Vector3 delta = finger.SwipeScreenDelta;
            SwipeDirection currentDirection;

            // Swipe vertical
            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            {
                if (delta.y > 0f)
                {
                    currentDirection = SwipeDirection.Up;
                }
                else
                {
                    currentDirection = SwipeDirection.Down;
                }
            }
            // Swipe horizontal
            else
            {
                if (delta.x > 0f)
                {
                    currentDirection = SwipeDirection.Right;
                }
                else
                {
                    currentDirection = SwipeDirection.Left;
                }
            }

            this.complete = currentDirection == this.direction;
        }

        public override void Dispose()
        {
            base.Dispose();
            LeanTouch.OnFingerSwipe -= FingerSwipeHandler;
        }
    }
}