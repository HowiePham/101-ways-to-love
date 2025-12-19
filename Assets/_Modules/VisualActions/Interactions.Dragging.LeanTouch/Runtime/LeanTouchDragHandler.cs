using Lean.Common;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Interactions.Dragging
{
    [RequireComponent(typeof(LeanFingerDown))]
    [RequireComponent(typeof(LeanSelectByFinger))]
    [TypeInfoBox("Drag Handler — receives user input and invokes events via Lean package.")]
    public class LeanTouchDragHandler : BaseDragHandler
    {
        [SerializeField] private LeanSelectByFinger leanSelectByFinger;

        private BaseDraggable currentDraggable;
        private bool isDragging;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.leanSelectByFinger == null)
            {
                this.leanSelectByFinger = GetComponent<LeanSelectByFinger>();
            }
        }
#endif

        protected override void OnInit()
        {
        }

        protected override void OnActivated()
        {
            this.isDragging = false;
            this.leanSelectByFinger.OnSelected.AddListener(SelectHandler);
            this.leanSelectByFinger.OnDeselected.AddListener(DeselectHandler);
            LeanTouch.OnFingerUpdate += FingerUpdateHandler;
            LeanTouch.OnFingerUp += FingerUpHandler;
        }

        protected override void OnDeactivated()
        {
            this.leanSelectByFinger.OnSelected.RemoveListener(SelectHandler);
            this.leanSelectByFinger.OnDeselected.RemoveListener(DeselectHandler);
            LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
        }

        private void SelectHandler(LeanSelectable select)
        {
            if (this.isDragging) return;
            var draggable = select.GetComponent<BaseDraggable>();
            if (draggable == this.currentDraggable) return;
            this.currentDraggable = draggable;
            this.isDragging = true;
            InvokeStartDragEvent(draggable);
        }

        private void DeselectHandler(LeanSelectable select)
        {
            if (!this.isDragging) return;
            InvokeEndDragEvent(this.currentDraggable);
            this.currentDraggable = null;
            this.isDragging = false;
        }

        private void FingerUpdateHandler(LeanFinger finger)
        {
            InvokeDragEvent(this.currentDraggable);
        }

        private void FingerUpHandler(LeanFinger finger)
        {
            if (this.currentDraggable == null)
            {
                this.leanSelectByFinger.DeselectAll();
            }
            else
            {
                this.leanSelectByFinger.Deselect(this.currentDraggable.GetComponent<LeanSelectable>());
            }
        }
    }
}