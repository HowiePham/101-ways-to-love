using Mimi.EffectMaker.Core;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class MonoRandomRotateTweenEffect : MonoEffectMaker
    {
        [SerializeField][MainInput]
        private Transform targetTransform;
        [SerializeField]
        private Vector3 minValue;
        [SerializeField]
        private Vector3 maxValue;
        [SerializeField]
        private float duration = 0.3f;
        protected override IEffectMaker CreateEffectMaker()
        {
            return new RandomRotateTweenEffect(targetTransform, minValue, maxValue, duration);
        }
    }
}