using System.Collections.Generic;
using CW.Common;
using Lean.Common;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Interactions.Dragging
{
    [RequireComponent(typeof(LeanSelectableByFinger))]
    [TypeInfoBox("A draggable component for moving objects using the Lean package.")]
    public class LeanTouchDraggable : BaseDraggable
    {
        /// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
        //[SerializeField] private LeanFingerFilter fingerFilter = new(true);
        [SerializeField] private LeanSelectable selectable;
        /// <summary>
        /// The camera the translation will be calculated using.
        /// </summary>
        [SerializeField] private Camera renderCamera;

        /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
        /*public void AddFinger(LeanFinger finger)
        {
            this.fingerFilter.AddFinger(finger);
        }

        /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
        public void RemoveFinger(LeanFinger finger)
        {
            this.fingerFilter.RemoveFinger(finger);
        }

        /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
        public void RemoveAllFingers()
        {
            this.fingerFilter.RemoveAllFingers();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            this.fingerFilter.UpdateRequiredSelectable(gameObject);
        }
#endif
*/
        protected override void OnInit()
        {
            //this.fingerFilter.UpdateRequiredSelectable(gameObject);
            
        }

        protected override void OnActivated()
        {
            this.selectable.OnSelected.AddListener(OnSelected);
            LeanTouch.OnFingerUpdate += OnMove;
            this.selectable.OnDeselected.AddListener(OnDeselected);
        }

        void OnSelected(LeanSelect finger)
        {
            OnStartDrag();
        }

        void OnMove(LeanFinger finger)
        {
            if (!selectable.IsSelected)
            {
                return;
            }

            if (Transform is RectTransform)
            {
                TranslateUI();
            }
            else
            {
                Translate();
            }
            OnDrag();
        }

        void OnDeselected(LeanSelect finger)
        {
            OnEndDrag();
        }
        protected override void OnDeactivated()
        {
            this.selectable.OnSelected.RemoveListener(OnSelected);
            LeanTouch.OnFingerUpdate -= OnMove;
            this.selectable.OnDeselected.RemoveListener(OnDeselected);
        }

       /* private void Update()
        {
            List<LeanFinger> fingers = this.fingerFilter.UpdateAndGetFingers();
            Vector2 screenDelta = LeanGesture.GetScreenDelta(fingers);

            if (screenDelta != Vector2.zero)
            {
                if (Transform is RectTransform)
                {
                    TranslateUI();
                }
                else
                {
                    Translate();
                }
            }
        }*/

        private void TranslateUI()
        {
            Camera finalCamera = this.renderCamera;

            if (finalCamera == null)
            {
                var canvas = Transform.GetComponentInParent<Canvas>();

                if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    finalCamera = canvas.worldCamera;
                }
            }

            // Screen position of the transform
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(finalCamera, Transform.position);

            // Convert back to world space
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(Transform.parent as RectTransform, screenPoint,
                    finalCamera, out Vector3 worldPoint))
            {
                worldPoint.z = Transform.position.z;
                SetPosition(worldPoint);
            }
        }

        private void Translate()
        {
            Camera cam = CwHelper.GetCamera(this.renderCamera, gameObject);

            if (cam != null)
            {
                Vector3 targetPos = cam.ScreenToWorldPoint(Input.mousePosition);
                targetPos.z = Transform.position.z;
                SetPosition(targetPos);
            }
            else
            {
                Debug.LogError(
                    "Failed to find camera. Either tag your camera as MainCamera, or set one in this component.", this);
            }
        }
    }
}