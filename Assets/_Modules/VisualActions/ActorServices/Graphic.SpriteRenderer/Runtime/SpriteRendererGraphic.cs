

using Mimi.Actor.Graphic.Core;
using UnityEngine;

namespace Mimi.Actor.Graphic.SpriteRenderer
{
    public class SpriteRendererGraphic : IGraphic
    {
        private UnityEngine.SpriteRenderer _spriteRenderer;
        public int SortingOrder => _spriteRenderer.sortingOrder;
        public string SortingLayerName => _spriteRenderer.sortingLayerName;
        public IGraphicAsset CurrentGraphicAsset { get; private set; }
        
        public SpriteRendererGraphic(UnityEngine.SpriteRenderer spriteRenderer)
        {
            this._spriteRenderer = spriteRenderer;
            this.CurrentGraphicAsset = new SpriteGraphicAsset(spriteRenderer.sprite);
        }
        
        public void SetAssetGraphic(IGraphicAsset graphic)
        {
            Debug.Log("hello");
            if (graphic is not IGraphicAsset<Sprite> spriteAssetGraphic)
            {
                return;
            }
            Sprite targetSprite = spriteAssetGraphic.GetAssetGraphic();
            _spriteRenderer.sprite = targetSprite;
            (this.CurrentGraphicAsset as IGraphicAsset<Sprite>).SetAssetGraphic(targetSprite);
        }

        public void SetSortingOrder(int sortingOrder)
        {
            this._spriteRenderer.sortingOrder = sortingOrder;
        }

        public void SetSortingLayerName(string sortingLayerName)
        {
            this._spriteRenderer.sortingLayerName = sortingLayerName;
        }

        public void Show()
        {
            _spriteRenderer.enabled = true;
        }

        public void Hide()
        {
            _spriteRenderer.enabled = false;
        }

        public void SetAlpha(float alpha)
        {
            var oldColor = _spriteRenderer.color;
            oldColor.a = alpha;
            _spriteRenderer.color = oldColor;
        }
    }
}