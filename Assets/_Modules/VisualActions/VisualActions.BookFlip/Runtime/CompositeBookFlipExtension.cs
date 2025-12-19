using UnityEngine;

namespace Mimi.VisualActions.BookFlip
{
    public class CompositeBookFlipExtension : MonoBookFlipExtension
    {
        [SerializeField] private MonoBookFlipExtension[] extensions;
        
        public override void Start()
        {
            foreach (var extension in this.extensions)
            {
                extension.Start();
            }
        }

        public override void FlipStart()
        {
            foreach (var extension in this.extensions)
            {
                extension.FlipStart();
            }
        }

        public override void FlipCompleted()
        {
            foreach (var extension in this.extensions)
            {
                extension.FlipCompleted();
            }
        }

        public override void PageRelease()
        {
            foreach (var extension in this.extensions)
            {
                extension.PageRelease();
            }
        }
    }
}