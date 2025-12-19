using UnityEngine;

namespace Mimi.VisualActions.BookFlip
{
    public abstract class MonoBookFlipExtension : MonoBehaviour,IBookFlipExtension
    {
        public abstract void Start();
        public abstract void FlipStart();
        public abstract void FlipCompleted();
        public abstract void PageRelease();
    }
}