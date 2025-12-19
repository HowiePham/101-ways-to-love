using Mimi.Actor.Graphic.Core;
using Spine.Unity;
using UnityEngine;

namespace Mimi.Actor.Graphic.Spine
{
    public class SpineGraphic : IGraphic
    {
        private SkeletonAnimation skeletonAnimation;
        private MeshRenderer meshRenderer;
        public int SortingOrder => meshRenderer.sortingOrder;
        public string SortingLayerName => meshRenderer.sortingLayerName;
        public IGraphicAsset CurrentGraphicAsset { get; }

        public SpineGraphic(SkeletonAnimation skeletonAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
            this.meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();
            this.CurrentGraphicAsset = new SkeletonDataGraphicAsset(this.skeletonAnimation.skeletonDataAsset);
        }
        public void SetAssetGraphic(IGraphicAsset graphic)
        {
            if (graphic is not IGraphicAsset<SkeletonDataAsset> spineGraphicAsset)
            {
                return;
            }
            var skeletonData = spineGraphicAsset.GetAssetGraphic();
            this.skeletonAnimation.skeletonDataAsset = skeletonData;
            this.skeletonAnimation.Initialize(true);
            (this.CurrentGraphicAsset as IGraphicAsset<SkeletonDataAsset>).SetAssetGraphic(skeletonData);
        }

        public void SetSortingOrder(int sortingOrder)
        {
            meshRenderer.sortingOrder = sortingOrder;
        }

        public void SetSortingLayerName(string sortingLayerName)
        {
            meshRenderer.sortingLayerName = sortingLayerName;
        }

        public void Show()
        {
            skeletonAnimation.enabled = true;
        }

        public void Hide()
        {
            skeletonAnimation.enabled = false;
        }

        public void SetAlpha(float alpha)
        {
            skeletonAnimation.Skeleton.A = alpha;
            foreach (var slot in skeletonAnimation.Skeleton.Slots)
            {
                slot.A = alpha;
            }
        }
    }
}