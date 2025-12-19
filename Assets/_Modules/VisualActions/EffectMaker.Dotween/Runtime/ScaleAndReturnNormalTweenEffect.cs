using DG.Tweening;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class ScaleAndReturnNormalTweenEffect: BaseDotweenEffect
    {
        private Transform target;
        private Vector3 targetScale;
        private float duration;
        private Ease ease;
        

        public ScaleAndReturnNormalTweenEffect(Transform target, Vector3 targetScale, float duration, Ease ease)
        {
            this.target = target;
            this.targetScale = targetScale;
            this.duration = duration;
            this.ease = ease;
        }
        

        protected override Tween CreateTween()
        {
            var intialScale = this.target.localScale;
            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(this.target.DOScale(targetScale, this.duration / 2).SetEase(this.ease));
            mySequence.Append(this.target.DOScale(intialScale, this.duration/2).SetEase(this.ease));
            return mySequence;
        }

      
    }
}