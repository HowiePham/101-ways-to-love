using System.Collections.Generic;
using Lean.Touch;
using Mimi.VisualActions.DragHold;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    [TypeInfoBox("Rotates an object around a point based on progress and direction.")]
    public class UpdateRotateAroundGO : BaseUpdateProgress
    {
        public enum RotationDirection
        {
            None = 0,
            Clockwise = 1,
            CounterClockwise = 2
        }

        [SerializeField] private float rotationSpeed;
        [SerializeField] private Transform rotatedTransform;
        [SerializeField] private bool isClockwise;
        [SerializeField] private bool isRotateManyDirection;

        private Vector3[] bufferPosition = new Vector3[30];

        /// <summary>
        /// Sets the object to rotate.
        /// </summary>
        /// <param name="obj">The Transform of the object to rotate.</param>
        public void SetRotatedObject(Transform obj)
        {
            this.rotatedTransform = obj.transform;
        }

        /// <summary>
        /// Updates rotation based on progress and finger position.
        /// </summary>
        /// <param name="progress">The current progress value.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <param name="isFingerDown">Whether the finger is down.</param>
        /// <param name="isIncreaseTime">Whether time is increasing.</param>
        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            UpdatePosition(finger.ScreenPosition);
            var sum = GetRotationDirection();
            if (isRotateManyDirection)
            {
                RotateManyDirections(sum);
            }
            else
            {
                RotateOneDirection(sum);
            }
        }

        /// <summary>
        /// Rotates in one direction based on rotation direction.
        /// </summary>
        /// <param name="rotationDirection">The direction to rotate.</param>
        private void RotateOneDirection(RotationDirection rotationDirection)
        {
            if (rotationDirection != RotationDirection.None)
            {
                rotatedTransform.RotateAround(rotatedTransform.position, rotatedTransform.forward,
                    rotationSpeed * Time.deltaTime * (isClockwise ? -1f : 1f));
            }
        }

        /// <summary>
        /// Rotates in multiple directions based on rotation direction.
        /// </summary>
        /// <param name="rotationDirection">The direction to rotate.</param>
        private void RotateManyDirections(RotationDirection rotationDirection)
        {
            if (rotationDirection == RotationDirection.CounterClockwise)
            {
                rotatedTransform.RotateAround(rotatedTransform.position, rotatedTransform.forward,
                    rotationSpeed * Time.deltaTime);
            }
            else if (rotationDirection == RotationDirection.Clockwise)
            {
                rotatedTransform.RotateAround(rotatedTransform.position, rotatedTransform.forward,
                    -rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Updates the position buffer with new finger position.
        /// </summary>
        /// <param name="newPosition">The new position to add.</param>
        private void UpdatePosition(Vector3 newPosition)
        {
            for (int i = bufferPosition.Length - 1; i > 0; i--)
            {
                bufferPosition[i] = bufferPosition[i - 1];
            }

            bufferPosition[0] = newPosition;
        }

        /// <summary>
        /// Determines the rotation direction from position buffer.
        /// </summary>
        /// <returns>The detected rotation direction.</returns>
        public RotationDirection GetRotationDirection()
        {
            if (bufferPosition == null || bufferPosition.Length < 2)
                return RotationDirection.None;

            Vector2 centroid = Vector2.zero;
            foreach (Vector2 pos in bufferPosition)
            {
                centroid += pos;
            }

            centroid /= bufferPosition.Length;

            List<float> angles = new List<float>();
            foreach (Vector2 pos in bufferPosition)
            {
                float angle = Mathf.Atan2(pos.y - centroid.y, pos.x - centroid.x);
                angles.Add(angle);
            }

            float totalRotation = 0f;
            for (int i = 1; i < angles.Count; i++)
            {
                float delta = angles[i] - angles[i - 1];
                if (delta > Mathf.PI)
                    delta -= 2 * Mathf.PI;
                else if (delta < -Mathf.PI)
                    delta += 2 * Mathf.PI;
                totalRotation += delta;
            }

            float threshold = 0.1f;
            if (Mathf.Abs(totalRotation) < threshold)
                return RotationDirection.None;
            else if (totalRotation > 0)
                return RotationDirection.CounterClockwise;
            else
                return RotationDirection.Clockwise;
        }

        /// <summary>
        /// Completes the progress (no action).
        /// </summary>
        public override void CompleteProgress()
        {

        }

        /// <summary>
        /// Initializes the progress updater.
        /// </summary>
        public override void OnInit()
        {

        }
    }
}