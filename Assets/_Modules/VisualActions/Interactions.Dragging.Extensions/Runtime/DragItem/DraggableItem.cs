using System;
using Mimi.Actor.Graphic.Core;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Data;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class DraggableItem : MonoBehaviour
    {
        [SerializeField] private Transform itemTransform;
        [SerializeField] private MonoItemDragExtension dragExtension;
        [SerializeField] private BaseMonoGraphic graphic;
        private bool isPickUp = false;
        public bool IsPickUp => isPickUp;
        public Vector3 InitialPosition { private set; get; }
        public Transform InitialItemParent { private set; get; }
        
        public BaseMonoGraphic Graphic => graphic;

        public void Init()
        {
            InitialItemParent = this.itemTransform.parent;
            dragExtension?.OnInit(this);
        }

        public void UpdateInitialPosition()
        {
            this.InitialPosition = this.itemTransform.position;
        }

        public Transform GetTransform()
        {
            return itemTransform;
        }

        public void PickUp(Transform parent, Vector3 localPosition)
        {
            isPickUp = true;
            SetParent(parent);
            SetLocalPosition(localPosition);
            dragExtension?.OnPickUp(this);
        }
        public void SetParent(Transform parent)
        {
            this.itemTransform.SetParent(parent);
        }

        public void SetPosition(Vector3 position)
        {
            this.itemTransform.position = position;
        }

        public void SetLocalPosition(Vector3 localPosition)
        {
            this.itemTransform.localPosition = localPosition;
        }

        public Vector3 GetPosition()
        {
            return this.itemTransform.position;
        }

        public void Return()
        {
            isPickUp = false;
            this.SetParent(InitialItemParent);
            this.SetPosition(InitialPosition);
            dragExtension?.OnReturn(this);
        }

        public void Completed()
        {
            this.SetParent(InitialItemParent);
        }
    }
}