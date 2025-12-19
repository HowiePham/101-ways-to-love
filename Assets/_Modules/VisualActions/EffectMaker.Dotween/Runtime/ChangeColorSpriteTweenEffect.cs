using DG.Tweening;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class ChangeColorSpriteTweenEffect : BaseDotweenEffect
    {
        private SpriteRenderer _spriteRenderer;
        private Color targetColor;
        private float duration;
        private Ease ease;

        public ChangeColorSpriteTweenEffect(SpriteRenderer spriteRenderer, Color targetColor, float duration, Ease ease)
        {
            _spriteRenderer = spriteRenderer;
            this.targetColor = targetColor;
            this.duration = duration;
            this.ease = ease;
        }


        protected override Tween CreateTween()
        {
            return _spriteRenderer.DOColor(targetColor, duration).SetEase(ease);
        }
    }
}