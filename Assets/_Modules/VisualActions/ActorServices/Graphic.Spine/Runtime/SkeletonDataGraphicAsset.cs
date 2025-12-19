using Mimi.Actor.Graphic.Core;
using Spine.Unity;

namespace Mimi.Actor.Graphic.Spine
{
    public class SkeletonDataGraphicAsset : BaseGenericGraphicAsset<SkeletonDataAsset>
    { 
        public SkeletonDataGraphicAsset(SkeletonDataAsset skeletonDataAsset)
        {
            SetAssetGraphic(skeletonDataAsset);
        }
    }
}