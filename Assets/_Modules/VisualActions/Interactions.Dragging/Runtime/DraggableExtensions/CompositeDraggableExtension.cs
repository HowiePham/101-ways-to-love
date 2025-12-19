using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Interactions.Dragging.DraggableExtensions
{
    public class CompositeDraggableExtension : MonoDraggableExtension, IParentDraggableExtension
    {
        [SerializeField] private MonoDraggableExtension[] draggableExtensions;

        public override void Init(BaseDraggable draggable)
        {
            foreach (var draggableExtension in this.draggableExtensions)
            {
                draggableExtension.Init(draggable);
            }
        }

        public override void StartDrag()
        {
            foreach (var draggableExtension in this.draggableExtensions)
            {
                draggableExtension.StartDrag();
            }
        }

        public override void Drag()
        {
            foreach (var draggableExtension in this.draggableExtensions)
            {
                draggableExtension.Drag();
            }
        }

        public override void EndDrag()
        {
            foreach (var draggableExtension in this.draggableExtensions)
            {
                draggableExtension.EndDrag();
            }
        }

        public IDraggableExtension GetExtension<T>() where T : IDraggableExtension
        {
            foreach (var extension in this.draggableExtensions)
            {
                if (extension.GetType() == typeof(T))
                {
                    return extension;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        [Button]
        private void GetAllExtensions()
        {
            this.draggableExtensions = GetComponentsInChildren<MonoDraggableExtension>();
        }
#endif
    }
}