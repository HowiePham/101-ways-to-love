using DG.Tweening;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class ChangeColorLineRendererTweenEffect : BaseDotweenEffect
    {
        private LineRenderer _lineRenderer;
        private Color startColor;
        private Color targetColor;
        private float duration;
        private Ease ease;

        public ChangeColorLineRendererTweenEffect(LineRenderer lineRenderer, Color startColor, Color targetColor, float duration,
            Ease ease)
        {
            this._lineRenderer = lineRenderer;
            this.startColor = startColor;
            this.targetColor = targetColor;
            this.duration = duration;
            this.ease = ease;
        }
        protected override Tween CreateTween()
        {
            return _lineRenderer.DOColor(new Color2(startColor, startColor), new Color2(targetColor,targetColor), duration).SetEase(ease);
        }
    }
}