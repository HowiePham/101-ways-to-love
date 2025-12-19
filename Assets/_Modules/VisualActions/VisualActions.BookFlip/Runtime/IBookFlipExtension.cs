namespace Mimi.VisualActions.BookFlip
{
    public interface IBookFlipExtension
    {
        public void Start();
        public void FlipStart();
        public void FlipCompleted();
        public void PageRelease();
    }
}