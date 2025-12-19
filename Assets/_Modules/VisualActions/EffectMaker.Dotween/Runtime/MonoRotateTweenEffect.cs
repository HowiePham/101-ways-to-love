using Mimi.EffectMaker.Core;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class MonoRotateTweenEffect : MonoEffectMaker
    {
        [SerializeField][MainInput]
        private Transform targetTransform;
        [SerializeField]
        private Vector3 targetRotation;
      
        [SerializeField]
        private float duration = 0.3f;
        protected override IEffectMaker CreateEffectMaker()
        {
            return new RotateTweenEffect(targetTransform, targetRotation, duration);
        }
    }
}