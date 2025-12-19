using System.Linq;
using UnityEngine;

namespace Mimi.Actor.Graphic.Core
{
    public class CompositeGraphic : IGraphic
    {
        private IGraphic[] graphics;

        public int SortingOrder
        {
            get
            {
                var result = graphics.Min(x => x.SortingOrder);
                return result;
            }
        }
        public string SortingLayerName => graphics[0].SortingLayerName;
        public IGraphicAsset CurrentGraphicAsset => graphics[0].CurrentGraphicAsset;

        public CompositeGraphic(IGraphic[] graphics)
        {
            this.graphics = graphics;
        }

        public void SetGraphics(IGraphic[] graphics)
        {
            this.graphics = graphics;
        }
        public void SetAssetGraphic(IGraphicAsset graphic)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].SetAssetGraphic(graphic);
            }
        }

        public void SetSortingOrder(int sortingOrder)
        {
            int minSortingOrder = this.SortingOrder;
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].SetSortingOrder(sortingOrder + graphics[i].SortingOrder - minSortingOrder);
                Debug.Log(sortingOrder + graphics[i].SortingOrder - minSortingOrder);
            }
        }

        public void SetSortingLayerName(string sortingLayerName)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].SetSortingLayerName(sortingLayerName);
            }
        }

        public void Show()
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].Show();
            }
        }

        public void Hide()
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].Hide();
            }
        }

        public void SetAlpha(float alpha)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].SetAlpha(alpha);
            }
        }
    }
}