using DG.Tweening;
using Mimi.EffectMaker.Core;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class MonoChangeColorSpriteTweenEffect : MonoEffectMaker
    {
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private Color targetColor;
        [SerializeField]
        private float duration = 0.3f;
        [SerializeField]
        private Ease ease;
        protected override IEffectMaker CreateEffectMaker()
        {
            return new ChangeColorSpriteTweenEffect(_spriteRenderer, targetColor, duration, ease);
        }
    }
}