using DG.Tweening;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class RandomRotateTweenEffect : BaseDotweenEffect
    {
        private Transform targetTransform;
        private Vector3 minValue;
        private Vector3 maxValue;
        private float duration;

        public RandomRotateTweenEffect(Transform targetTransform, Vector3 minValue, Vector3 maxValue, float duration)
        {
            this.targetTransform = targetTransform;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.duration = duration;
        }

        protected override Tween CreateTween()
        {
            var random = new Vector3(Random.Range(minValue.x, maxValue.x), Random.Range(minValue.y, maxValue.y), Random.Range(minValue.z, maxValue.z));
            return targetTransform.DORotate(random, duration);
        }
    }
}