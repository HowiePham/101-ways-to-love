using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace VisualFlow
{
    public class SpineHintGraphic : BaseHintGraphic
    {
        [SerializeField, Required] private SkeletonAnimation skeletonAnimation;

        private Renderer spineRenderer;

        private void Awake()
        {
            this.spineRenderer = this.skeletonAnimation.GetComponent<Renderer>();
        }

        public override void SetActive(bool active)
        {
            this.spineRenderer.enabled = active;
        }

        public override void SetColor(Color color)
        {
            this.skeletonAnimation.skeleton.SetColor(color);
        }
    }
}