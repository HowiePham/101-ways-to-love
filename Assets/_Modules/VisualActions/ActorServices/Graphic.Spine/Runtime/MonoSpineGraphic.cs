using Mimi.Actor.Graphic.Core;
using Spine.Unity;
using UnityEngine;

namespace Mimi.Actor.Graphic.Spine
{
    public class MonoSpineGraphic : BaseMonoGraphic
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        protected override IGraphic CreateGraphic()
        {
            return new SpineGraphic(skeletonAnimation);
        }
    }
}