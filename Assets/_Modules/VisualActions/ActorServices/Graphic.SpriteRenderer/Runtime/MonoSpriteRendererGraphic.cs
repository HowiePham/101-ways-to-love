using Mimi.Actor.Graphic.Core;
using UnityEngine;

namespace Mimi.Actor.Graphic.SpriteRenderer
{
    public class MonoSpriteRendererGraphic : BaseMonoGraphic
    {
        [SerializeField] private UnityEngine.SpriteRenderer _spriteRenderer;
        protected override IGraphic CreateGraphic()
        {
            return new SpriteRendererGraphic(this._spriteRenderer);
        }
    }
}