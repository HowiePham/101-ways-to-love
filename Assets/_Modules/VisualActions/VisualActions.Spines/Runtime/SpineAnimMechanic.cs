using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    public abstract class SpineAnimMechanic : VisualAction
    {
        [SerializeField] protected SkeletonAnimation skeletonAnimation;
        [SerializeField, SpineAnimation(dataField = "skeletonAnimation")]
        protected new string animation;

        public string Animation => this.animation;

        public SkeletonAnimation SkeletonAnimation
        {
            get => this.skeletonAnimation;
            set => this.skeletonAnimation = value;
        }
    }
}