using System.Collections;
using DG.Tweening;
#if UNITY_EDITOR
using Mimi.Debugging.UnityGizmos;
#endif
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class DragRotateExtenstion : MonoDragExtension
    {
        [SerializeField] private BaseArea selectArea;
        [SerializeField] private Transform rotatedTransform;
        [SerializeField] private float rotateSpeed = 30f;
        [SerializeField] private float winAngle = 0;
        [SerializeField] private float startAngle = 50f;
        [SerializeField] private float delta;
        [SerializeField] private float minAngle;
        [SerializeField] private float maxAngle;
        
        private Camera mainCamera;
        private Vector2 pivotScreenPosition;
        private bool completed;
        private bool dragging;
        private bool selected;
        private bool playEffect;
        private Vector2 prevFingerScreenPosition;
        public override void Init()
        {
            mainCamera = Camera.main;
            pivotScreenPosition = mainCamera.WorldToScreenPoint(rotatedTransform.position);
        }

        public override void Start()
        {
            completed = false;
            dragging = false;
            selected = false;
            playEffect = false;
            rotatedTransform.eulerAngles = new Vector3(rotatedTransform.eulerAngles.x, rotatedTransform.eulerAngles.y, startAngle);
        }

        public override void StartDrag(BaseDraggable draggable)
        {
            if (selectArea.ContainsScreenPosition(Input.mousePosition, mainCamera))
            {
                prevFingerScreenPosition = Input.mousePosition;
                selected = true;
            }
        }

        public override void Drag(BaseDraggable draggable)
        {
            var currentPosition = (Vector2)Input.mousePosition;
            Vector2 offset = currentPosition - prevFingerScreenPosition;
            dragging = offset.SqrMagnitude() >= Mathf.Epsilon;
            if (dragging && selected)
            {
                
                Vector2 fingerDirection = GetFingerDirectionFromTargetPivot(currentPosition);

                float prevFingerAngle = Vector2.SignedAngle(Vector2.up, prevFingerScreenPosition - pivotScreenPosition);
                float currentFingerAngle = Vector2.SignedAngle(Vector2.up, currentPosition- pivotScreenPosition);
                //Debug.Log("Current Angle "+currentFingerAngle);
                if (currentFingerAngle >= minAngle && currentFingerAngle <= maxAngle)
                {
                    rotatedTransform.eulerAngles = new Vector3(rotatedTransform.eulerAngles.x, rotatedTransform.eulerAngles.y, currentFingerAngle);
                }
                
                /*if (prevFingerAngle < currentFingerAngle)
                {
                    rotatedTransform.RotateAround(rotatedTransform.position, rotatedTransform.forward, rotateSpeed * Time.deltaTime);
                }
                else
                {
                    rotatedTransform.RotateAround(rotatedTransform.position, rotatedTransform.forward, -rotateSpeed * Time.deltaTime);
                }*/
            
            
#if UNITY_EDITOR
                VisualDebugger.DebugArrow(rotatedTransform.position, fingerDirection, Color.red, 0.1f);
                VisualDebugger.DebugArrow(rotatedTransform.position, rotatedTransform.right, Color.blue, 0.1f);
#endif
            }

            prevFingerScreenPosition = currentPosition;
        }
      
        
        private Vector2 GetFingerDirectionFromTargetPivot(Vector2 fingerScreenPosition)
        {
            Vector2 pivotScreenPosition = mainCamera.WorldToScreenPoint(rotatedTransform.position);
            return fingerScreenPosition - pivotScreenPosition;
        }

        public override void EndDrag(BaseDraggable draggable)
        {
            dragging = false;
            selected = false;
        }

        public override void OnCompleted(BaseDraggable draggable)
        {
            StartCoroutine(CompleteRotate());
        }

        private IEnumerator CompleteRotate()
        {
            rotatedTransform.DORotate(new Vector3(rotatedTransform.eulerAngles.x, rotatedTransform.eulerAngles.y, winAngle),
                0.3f, RotateMode.Fast);
            yield return new WaitForSeconds(0.3f);
        }
        
        public bool CheckWinCondition()
        {
            if (winAngle < 0)
            {
                return (rotatedTransform.eulerAngles.z % 360 < 360 + winAngle + delta && rotatedTransform.eulerAngles.z % 360 > 360 + winAngle - delta);
            }
            else if (winAngle == 0)
            {
                return (rotatedTransform.eulerAngles.z % 360 < winAngle + delta && rotatedTransform.eulerAngles.z % 360 >= winAngle) || (rotatedTransform.eulerAngles.z % 360 <= 360 && rotatedTransform.eulerAngles.z % 360 > 360 - delta);
            }

            return (rotatedTransform.eulerAngles.z < winAngle + delta && rotatedTransform.eulerAngles.z > winAngle - delta);
        }
        
        public override void OnFailed(BaseDraggable draggable)
        {
           
        }
    }
}