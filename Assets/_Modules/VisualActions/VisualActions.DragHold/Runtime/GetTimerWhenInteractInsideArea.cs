using System;
using System.Collections.Generic;
using Lean.Common;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.DragHold
{
    [TypeInfoBox("Tracks time spent interacting inside a specified area.")]
    public class GetTimerWhenInteractInsideArea : MonoBehaviour
    {
        public enum DragDirection
        {
            Any,
            Left,
            Right,
            Up,
            Down
        }

        [SerializeField] private BaseArea targetArea;
        [SerializeField] private LeanSelectable leanSelectable;
        [SerializeField] private bool moveInsideArea = false;
        [SerializeField] private bool isFingerDownThenMoveOver;

        [SerializeField, ShowIf("moveInsideArea")]
        private DragDirection dragDirection;

        private bool isFingerDown;
        protected bool isUseTool => leanSelectable != null;
        private Vector3 currentFingerPos;
        private Vector3 prevFingerPos;
        public float insideTime = 0;
        public Action<float, LeanFinger, bool, bool> OnUpdateProgress;
        
        /// <summary>
        /// Registers touch event handlers.
        /// </summary>
        private void OnEnable()
        {
            LeanTouch.OnFingerUpdate += FingerUpdateHandler;
            LeanTouch.OnFingerDown += FingerDownHandler;
            LeanTouch.OnFingerUp += FingerUpHandler;
        }

        /// <summary>
        /// Unregisters touch event handlers.
        /// </summary>
        private void OnDisable()
        {
            LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
        }

        /// <summary>
        /// Sets the target area for interaction.
        /// </summary>
        /// <param name="area">The BaseArea to set.</param>
        public void SetArea(BaseArea area)
        {
            this.targetArea = area;
        }

        /// <summary>
        /// Handles finger release.
        /// </summary>
        /// <param name="finger">The LeanFinger input data.</param>
        private void FingerUpHandler(LeanFinger finger)
        {
            isFingerDown = false;
        }

        /// <summary>
        /// Handles finger press.
        /// </summary>
        /// <param name="finger">The LeanFinger input data.</param>
        private void FingerDownHandler(LeanFinger finger)
        {
            prevFingerPos = finger.ScreenPosition;
            isFingerDown = true;
            if (isFingerDownThenMoveOver)
            {
                isFingerDown = targetArea.ContainsWorldSpace(GetCheckPos(finger));
            }
        }

        /// <summary>
        /// Handles finger movement updates.
        /// </summary>
        /// <param name="finger">The LeanFinger input data.</param>
        private void FingerUpdateHandler(LeanFinger finger)
        {
            if (!isFingerDown) return;
            currentFingerPos = finger.ScreenPosition;


            var isIncreaseTime = CheckIncreaseTime(GetCheckPos(finger), finger);

            prevFingerPos = finger.ScreenPosition;
            OnUpdateProgress?.Invoke(insideTime, finger, isFingerDown, isIncreaseTime);
        }

        /// <summary>
        /// Gets the position to check for area containment.
        /// </summary>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <returns>The check position in world space.</returns>
        private Vector3 GetCheckPos(LeanFinger finger)
        {
            Vector3 checkPos = !isUseTool
                ? Camera.main.ScreenToWorldPoint(finger.ScreenPosition)
                : leanSelectable.transform.position;
            checkPos.z = 0;
            return checkPos;
        }

        /// <summary>
        /// Checks if time should increase based on position and movement.
        /// </summary>
        /// <param name="checkPos">The position to check.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <returns>True if time should increase, false otherwise.</returns>
        bool CheckIncreaseTime(Vector3 checkPos, LeanFinger finger)
        {
            if (!isFingerDown) return false;
            if (isUseTool && !leanSelectable.IsSelected) return false;
            if (!isFingerDownThenMoveOver && !targetArea.ContainsWorldSpace(checkPos))
            {
                return false;
            }

            if (moveInsideArea && currentFingerPos == prevFingerPos) return false;
            if (moveInsideArea && dragDirection != GetDragDirection(prevFingerPos, finger.ScreenPosition)) return false;

            insideTime += Time.deltaTime;
            return true;
        }

        /// <summary>
        /// Determines the drag direction.
        /// </summary>
        /// <param name="prevPos">Previous finger position.</param>
        /// <param name="currentPos">Current finger position.</param>
        /// <returns>The detected drag direction.</returns>
        private DragDirection GetDragDirection(Vector3 prevPos, Vector3 currentPos)
        {
            DragDirection dir;
            if (dragDirection == DragDirection.Any) return DragDirection.Any;

            if (Mathf.Abs(prevPos.x - currentPos.x) >
                Mathf.Abs(prevPos.y - currentPos.y))
            {
                if (prevPos.x > currentPos.x)
                {
                    dir = DragDirection.Left;
                }
                else
                {
                    dir = DragDirection.Right;
                }
            }
            else
            {
                if (prevPos.y > currentPos.y)
                {
                    dir = DragDirection.Down;
                }
                else
                {
                    dir = DragDirection.Up;
                }
            }

            return dir;
        }

        /// <summary>
        /// Increases the inside time by a specified amount.
        /// </summary>
        /// <param name="time">The time to add.</param>
        public void IncreaseTime(float time)
        {
            insideTime += time;
        }
    }
}