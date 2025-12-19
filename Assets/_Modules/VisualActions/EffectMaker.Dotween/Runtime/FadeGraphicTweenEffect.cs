using DG.Tweening;
using Mimi.Actor.Graphic.Core;

namespace Mimi.EffectMaker.Dotween
{
    public class FadeGraphicTweenEffect : BaseDotweenEffect
    {
        private IGraphic graphic;
        private float startAlpha;
        private float endAlpha;
        private float duration;
        private Ease ease;
        private float currentProgression;
        
        public FadeGraphicTweenEffect(IGraphic graphic, float startAlpha, float endAlpha, float duration, Ease ease)
        {
            this.graphic = graphic;
            this.startAlpha = startAlpha;
            this.endAlpha = endAlpha;
            this.duration = duration;
            this.ease = ease;
        }
        protected override Tween CreateTween()
        {
            this.currentProgression = startAlpha;
            return DOTween.To(
                    () => currentProgression, // getter: lấy giá trị hiện tại
                    x => currentProgression = x, // setter: gán giá trị mới
                    endAlpha, // to   (giá trị đích)
                    duration // duration
                )
                .SetEase(ease) // easing
                .OnUpdate(() => graphic.SetAlpha(currentProgression));
        }
    }
}