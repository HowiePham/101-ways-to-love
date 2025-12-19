using DG.Tweening;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class RotateTweenEffect : BaseDotweenEffect
    {
        private Transform targetTransform;
        private Vector3 target;
        private float duration;

        public RotateTweenEffect(Transform targetTransform,  Vector3 target, float duration)
        {
            this.targetTransform = targetTransform;
            this.target = target;
            this.duration = duration;
        }

        protected override Tween CreateTween()
        {
            return targetTransform.DORotate(target, duration);
        }
    }
}