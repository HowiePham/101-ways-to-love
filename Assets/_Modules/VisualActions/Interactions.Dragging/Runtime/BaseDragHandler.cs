using System;
using UnityEngine;

namespace Mimi.Interactions.Dragging
{
    public abstract class BaseDragHandler : MonoBehaviour
    {
        public event Action<BaseDraggable> OnStartDrag;
        public event Action<BaseDraggable> OnDragging;
        public event Action<BaseDraggable> OnEndDrag;

        private void OnEnable()
        {
            OnActivated();
        }

        private void OnDisable()
        {
            OnDeactivated();
        }

        private void Awake()
        {
            OnInit();
        }

        protected abstract void OnInit();
        protected abstract void OnActivated();
        protected abstract void OnDeactivated();

        protected void InvokeStartDragEvent(BaseDraggable draggable)
        {
            OnStartDrag?.Invoke(draggable);
        }

        protected void InvokeDragEvent(BaseDraggable draggable)
        {
            OnDragging?.Invoke(draggable);
        }

        protected void InvokeEndDragEvent(BaseDraggable draggable)
        {
            OnEndDrag?.Invoke(draggable);
        }
    }
}