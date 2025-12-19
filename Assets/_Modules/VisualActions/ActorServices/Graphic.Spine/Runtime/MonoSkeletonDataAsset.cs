using Mimi.Actor.Graphic.Core;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Mimi.Actor.Graphic.Spine
{
    public class MonoSkeletonDataAsset : MonoGraphicAsset<SkeletonDataAsset>
    {
        [SerializeField,OnValueChanged("SkeletonDataAssetChanged")] private SkeletonDataAsset skeletonDataAsset;
        protected override IGraphicAsset<SkeletonDataAsset> CreateAssetGraphic()
        {
            return new SkeletonDataGraphicAsset(skeletonDataAsset);
        }

        void SkeletonDataAssetChanged(SkeletonDataAsset newAsset)
        {
            SetAssetGraphic(newAsset);
        }
    }
}