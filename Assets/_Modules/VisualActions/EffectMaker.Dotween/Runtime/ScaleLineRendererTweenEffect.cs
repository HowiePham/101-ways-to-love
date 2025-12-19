using DG.Tweening;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class ScaleLineRendererTweenEffect : BaseDotweenEffect
    {
        private LineRenderer target;
        private float initialWidth;
        private float targetWidth;
        private float duration;
        private Ease ease;
        private float currentWidth;

        public ScaleLineRendererTweenEffect(LineRenderer target, float initialScale, float targetScale, float duration, Ease ease)
        {
            this.target = target;
            this.initialWidth = initialScale;
            this.targetWidth = targetScale;
            this.duration = duration;
            this.ease = ease;
           
        }
        

        protected override Tween CreateTween()
        {
            this.target.startWidth = this.initialWidth;
            this.target.endWidth = this.initialWidth;
            this.currentWidth = this.initialWidth;
            return DOTween.To(
                    () => currentWidth, // getter: lấy giá trị hiện tại
                    x => currentWidth = x, // setter: gán giá trị mới
                    targetWidth, // to   (giá trị đích)
                    duration // duration
                )
                .SetEase(ease) // easing
                .OnUpdate(() =>
                {
                    this.target.startWidth = this.currentWidth;
                    this.target.endWidth = this.currentWidth;
                });
        }

      
    }
}