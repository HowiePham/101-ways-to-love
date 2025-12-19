using System.Collections.Generic;
using ActorServices.Graphic.Draggable.Runtime;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions;
using Mimi.VisualActions.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using VisualActions.Areas;

namespace Mimi.Interactions.Dragging.Extensions
{
    [TypeInfoBox("Extensions for dragging other items\nItems Transform: Array of all draggable items.\nCheck Area: Area used for drag checking.")]
    public class DraggableItemExtension : MonoDraggableExtension
    {
        [SerializeField] private Transform parentHolderTransform;
        [SerializeField] private List<DraggableItem> itemsTransform;
        [SerializeField] private BaseArea checkArea;
        [SerializeField] private bool isReturnWhenRelease;
        [FormerlySerializedAs("outputTransformField")] [SerializeField] private DraggableItemField outputField;
        
        private DraggableItem currentDraggableItem;
        private bool IsDragItem;
        private Transform initialItemParent;
        private Vector3 initialPosition;
        public override void Init(BaseDraggable draggable)
        {
            base.Init(draggable);
            IsDragItem = false;
            this.currentDraggableItem = null;
            foreach (var item in this.itemsTransform)
            {
                item.Init();
            }
        }
        
        public bool IsEmpty()
        {
            return currentDraggableItem == null;
        }
        
        public void ReturnCurrentItem()
        {
            this.IsDragItem = false;
            this.currentDraggableItem.Return();
            this.currentDraggableItem = null;
            this.outputField.SetValue(null);
        }
        
        public override void StartDrag()
        {
            foreach (var item in this.itemsTransform)
            {
                item.UpdateInitialPosition();
            }
        }

        public override void Drag()
        {
            //Debug.Log($"Is draggable null {draggable == null} Is Selected {draggable.IsSelected} Is DragItem {this.IsDragItem} IsEmpty {this.IsEmpty()}");
            if ( !this.BaseDraggable.IsSelected || this.IsDragItem || !IsEmpty())
            {
                return;
            }
            //Debug.Log("aaa");
            foreach (var item in this.itemsTransform)
            {
                if (item.GetTransform().gameObject.activeInHierarchy && this.checkArea.ContainsWorldSpace(item.GetPosition()))
                {
                    this.IsDragItem = true;
                    item.PickUp(parentHolderTransform, Vector3.zero);
                    this.currentDraggableItem = item;
                    this.outputField.SetValue(item);
                    return;
                }
            }
        }

        public void OnDragFailed(BaseDraggable draggable)
        {
            if (draggable != BaseDraggable ||
                draggable.IsSelected || !this.isReturnWhenRelease || !this.IsDragItem || this.currentDraggableItem ==null)
            {
                return;
            }
            ReturnCurrentItem();
        }

        public void OnDragSuccess(BaseDraggable draggable)
        {
            if (draggable != BaseDraggable || !IsDragItem || this.currentDraggableItem == null)
            {
                return;
            }

            this.IsDragItem = false;
            this.currentDraggableItem.Completed();
            this.itemsTransform.Remove(this.currentDraggableItem);
            this.currentDraggableItem = null;
        }
        public override void EndDrag()
        {
           
        }
        
    }
}