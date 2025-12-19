#if UNITY_EDITOR
using Mimi.Debugging.UnityGizmos;
#endif
using Mimi.Interactions.Dragging;
using Mimi.VisualActions.Attribute;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Deleting
{
    [TypeInfoBox("Extension to enable scratching only when a draggable tool is selected.")]
    public class UsingToolSmoothDeleteExtension : MonoSmoothDeleteExtension
    {
        [SerializeField] private BaseDraggable tool;
        [HideInBehaviourEditor]
        [SerializeField] private VisualSmoothDelete visualSmoothDelete;
        
        /// <summary>
        /// Disables scratching on delete start.
        /// </summary>
        /// <param name="position">The position where deletion starts.</param>
        public override void OnStartDelete(Vector3 position)
        {
            this.smoothDelete.ScratchCard.SetEnableScratching(false);
        }

        /// <summary>
        /// Enables scratching if tool is selected.
        /// </summary>
        /// <param name="position">The current deletion position.</param>
        public override void OnDelete(Vector3 position)
        {
            this.smoothDelete.ScratchCard.SetEnableScratching(tool.IsSelected);
        }

        /// <summary>
        /// Does nothing on delete stop.
        /// </summary>
        public override void OnStopDelete()
        {
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Draws gizmo bounds for brush in editor.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (visualSmoothDelete != null && visualSmoothDelete.MaskSpriteRenderer && 
                visualSmoothDelete.BrushMaskTexture != null && tool != null)
            {
                float scale = visualSmoothDelete.MaskSpriteRenderer.transform.localScale.x;
                Vector3 size = new Vector3(visualSmoothDelete.BrushMaskTexture.width / 100f, visualSmoothDelete.BrushMaskTexture.height / 100f, 1f);
                size *= visualSmoothDelete.EraseSize * scale;
                Vector3 offset = visualSmoothDelete.BrushOffset;
                Vector3 center = tool.transform.position + offset * scale / 100f;
                Bounds fuckBounds = new Bounds();
                fuckBounds.center = center;
                fuckBounds.size = size;
                VisualDebugger.DrawBounds(fuckBounds, Color.red);
            }
        }
        #endif
    }
}