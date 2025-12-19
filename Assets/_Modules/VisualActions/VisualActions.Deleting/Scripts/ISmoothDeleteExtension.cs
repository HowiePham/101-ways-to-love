using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Deleting
{
    public interface ISmoothDeleteExtension
    {
        /// <summary>
        /// Initializes extension with smooth delete instance.
        /// </summary>
        /// <param name="smoothDelete">The VisualSmoothDelete instance.</param>
        void OnInit(VisualSmoothDelete smoothDelete);


        void OnActionStart();
        /// <summary>
        /// Handles start of deletion.
        /// </summary>
        /// <param name="position">The position where deletion starts.</param>
        void OnStartDelete(Vector3 position);
        
        /// <summary>
        /// Handles ongoing deletion.
        /// </summary>
        /// <param name="position">The current deletion position.</param>
        void OnDelete(Vector3 position);
        
        /// <summary>
        /// Handles end of deletion.
        /// </summary>
        void OnStopDelete();
    }
}