namespace Mimi.Interactions.Dragging.Extensions
{
    public interface IItemDragExtension
    {
        public void OnInit(DraggableItem draggableItem);
        public void OnPickUp(DraggableItem draggableItem);
        
        public void OnReturn(DraggableItem draggableItem);
    }
}