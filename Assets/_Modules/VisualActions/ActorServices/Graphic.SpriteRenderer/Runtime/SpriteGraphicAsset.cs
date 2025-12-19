using Mimi.Actor.Graphic.Core;
using UnityEngine;

namespace Mimi.Actor.Graphic.SpriteRenderer
{
    public class SpriteGraphicAsset : BaseGenericGraphicAsset<Sprite>
    {
        public SpriteGraphicAsset(Sprite sprite)
        {
            SetAssetGraphic(sprite);
        }
    }
}