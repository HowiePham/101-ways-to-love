using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Deleting
{
    [TypeInfoBox("Composite extension delegating calls to multiple MonoSmoothDeleteExtensions.")]
    public class CompositeSmoothDeleteExtension : MonoSmoothDeleteExtension
    {
        [SerializeField] private MonoSmoothDeleteExtension[] extensions;
        
        /// <summary>
        /// Initializes all child extensions.
        /// </summary>
        /// <param name="smoothDelete">The VisualSmoothDelete instance.</param>
        public override void OnInit(VisualSmoothDelete smoothDelete)
        {
            base.OnInit(smoothDelete);
            foreach (var extension in extensions)
            {
                extension.OnInit(smoothDelete);
            }
        }

        /// <summary>
        /// Calls OnStartDelete on all extensions.
        /// </summary>
        /// <param name="position">The position where deletion starts.</param>
        public override void OnStartDelete(Vector3 position)
        {
            foreach (var extension in extensions)
            {
                extension.OnStartDelete(position);
            }
        }

        /// <summary>
        /// Calls OnDelete on all extensions.
        /// </summary>
        /// <param name="position">The current deletion position.</param>
        public override void OnDelete(Vector3 position)
        {
            foreach (var extension in extensions)
            {
                extension.OnDelete(position);
            }
        }

        /// <summary>
        /// Calls OnStopDelete on all extensions.
        /// </summary>
        public override void OnStopDelete()
        {
            foreach (var extension in extensions)
            {
                extension.OnStopDelete();
            }
        }
    }
}