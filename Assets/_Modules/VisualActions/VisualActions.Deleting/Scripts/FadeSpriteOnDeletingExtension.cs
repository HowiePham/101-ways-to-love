using UnityEngine;

namespace Mimi.VisualActions.Deleting
{
    public class FadeSpriteOnDeletingExtension : MonoSmoothDeleteExtension
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float targetAlpha;

        private float initialAlpha;

        public override void OnActionStart()
        {
            base.OnActionStart();
            initialAlpha = spriteRenderer.color.a;
        }

        public override void OnStartDelete(Vector3 position)
        {
            
        }

        public override void OnDelete(Vector3 position)
        {
            var color = spriteRenderer.color;
            color.a = initialAlpha + (targetAlpha - initialAlpha) * smoothDelete.Progress;
            spriteRenderer.color = color;
        }

        public override void OnStopDelete()
        {
           
        }
    }
}