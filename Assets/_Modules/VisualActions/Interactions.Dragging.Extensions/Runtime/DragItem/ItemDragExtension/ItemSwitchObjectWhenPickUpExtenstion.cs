using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class ItemSwitchObjectWhenPickUpExtenstion : MonoItemDragExtension
    {
        [SerializeField] private GameObject objectDeactive;
        [SerializeField] private GameObject objectActive;
        public override void OnInit(DraggableItem draggableItem)
        {
        }

        public override void OnPickUp(DraggableItem draggableItem)
        {
            objectDeactive?.SetActive(false);
            objectActive?.SetActive(true);
        }

        public override void OnReturn(DraggableItem draggableItem)
        {
            objectDeactive?.SetActive(true);
            objectActive?.SetActive(false);
        }
    }
}