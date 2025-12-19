using DG.Tweening;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class PunchScaleTweenEffect : BaseDotweenEffect
    {
        private Transform target;
        private Vector3 strength;
        private float duration;
        private Ease ease;
        public PunchScaleTweenEffect(Transform target, Vector3 strength, float duration, Ease ease)
        {
            this.target = target;
            this.strength = strength;
            this.duration = duration;
            this.ease = ease;
        }
        protected override Tween CreateTween()
        {
            return this.target.DOPunchScale(strength, this.duration).SetEase(this.ease);
        }
    }
}