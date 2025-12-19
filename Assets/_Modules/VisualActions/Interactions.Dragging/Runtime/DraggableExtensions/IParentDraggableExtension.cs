namespace Mimi.Interactions.Dragging.DraggableExtensions
{
    public interface IParentDraggableExtension
    {
        public IDraggableExtension GetExtension<T>() where T : IDraggableExtension;
    }
}