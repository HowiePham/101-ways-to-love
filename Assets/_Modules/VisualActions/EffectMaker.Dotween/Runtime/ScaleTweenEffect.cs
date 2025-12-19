using DG.Tweening;
using Mimi.EffectMaker.Core;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class ScaleTweenEffect : BaseDotweenEffect
    {
        private Transform target;
        private Vector3 initialScale;
        private Vector3 targetScale;
        private float duration;
        private Ease ease;
        

        public ScaleTweenEffect(Transform target, Vector3 initialScale, Vector3 targetScale, float duration, Ease ease)
        {
            this.target = target;
            this.initialScale = initialScale;
            this.targetScale = targetScale;
            this.duration = duration;
            this.ease = ease;
           
        }
        

        protected override Tween CreateTween()
        {
            this.target.localScale = this.initialScale;
            return this.target.DOScale(targetScale, this.duration).SetEase(this.ease);
        }

      
    }
}