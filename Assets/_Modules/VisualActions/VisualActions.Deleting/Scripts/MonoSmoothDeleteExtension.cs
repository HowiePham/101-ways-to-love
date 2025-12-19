using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Deleting
{
    public abstract class MonoSmoothDeleteExtension : MonoBehaviour, ISmoothDeleteExtension
    {
        protected VisualSmoothDelete smoothDelete { get; private set; }
        
        /// <summary>
        /// Initializes with smooth delete, stores reference.
        /// </summary>
        /// <param name="smoothDelete">The VisualSmoothDelete instance.</param>
        public virtual void OnInit(VisualSmoothDelete smoothDelete)
        {
            this.smoothDelete = smoothDelete;
        }

        public virtual void OnActionStart()
        {
            
        }


        /// <summary>
        /// Handles start of deletion.
        /// </summary>
        /// <param name="position">The position where deletion starts.</param>
        public abstract void OnStartDelete(Vector3 position);
        
        /// <summary>
        /// Handles ongoing deletion.
        /// </summary>
        /// <param name="position">The current deletion position.</param>
        public abstract void OnDelete(Vector3 position);
        
        /// <summary>
        /// Handles end of deletion.
        /// </summary>
        public abstract void OnStopDelete();
    }
}