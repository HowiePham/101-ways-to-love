using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Deleting
{
    [TypeInfoBox("A null implementation of MonoSmoothDeleteExtension that performs no actions.")]
    public class NullSmoothDeleteExtension : MonoSmoothDeleteExtension
    {
        /// <summary>
        /// Does nothing when deletion starts.
        /// </summary>
        /// <param name="position">The position where deletion starts.</param>
        public override void OnStartDelete(Vector3 position)
        {
            
        }

        /// <summary>
        /// Does nothing during deletion.
        /// </summary>
        /// <param name="position">The current deletion position.</param>
        public override void OnDelete(Vector3 position)
        {
        
        }

        /// <summary>
        /// Does nothing when deletion stops.
        /// </summary>
        public override void OnStopDelete()
        {
            
        }
    }
}