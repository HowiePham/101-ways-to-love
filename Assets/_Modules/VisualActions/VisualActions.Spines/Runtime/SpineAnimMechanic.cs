using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    public abstract class SpineAnimMechanic : VisualAction
    {
        [SerializeField] protected SkeletonAnimation skeletonAnimation;
        [SerializeField, SpineAnimation(dataField = "skeletonAnimation")]
        protected new string animation;

        [SerializeField] protected float timeScale = 1f;

        public string Animation => this.animation;

        public SkeletonAnimation SkeletonAnimation
        {
            get => this.skeletonAnimation;
            set => this.skeletonAnimation = value;
        }

        protected bool HasAnimation()
        {
            return SkeletonAnimation.SkeletonDataAsset != null && !string.IsNullOrEmpty(this.Animation);
        }
    }
}