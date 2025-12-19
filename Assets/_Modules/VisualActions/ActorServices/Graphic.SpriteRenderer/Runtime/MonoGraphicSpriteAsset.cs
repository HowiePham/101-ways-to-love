using Mimi.Actor.Graphic.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Actor.Graphic.SpriteRenderer
{
    public class MonoGraphicSpriteAsset : MonoGraphicAsset<Sprite>
    {
        [SerializeField,OnValueChanged("SpriteChanged")] private Sprite sprite;
        protected override IGraphicAsset<Sprite> CreateAssetGraphic()
        {
            return new SpriteGraphicAsset(sprite);
        }

        void SpriteChanged(Sprite changed)
        {
            SetAssetGraphic(changed);
        }
    }
}