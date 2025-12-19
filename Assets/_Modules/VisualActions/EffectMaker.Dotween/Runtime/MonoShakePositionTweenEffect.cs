using Mimi.EffectMaker.Core;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class MonoShakePositionTweenEffect : MonoEffectMaker
    {
        [MainInput]
        [SerializeField]
        private Transform targeTransform;
        [SerializeField]
        private Vector3 strength = Vector3.one;
        [SerializeField]
        private float duration = 0.5f;
        [SerializeField]
        private int vibrato = 10;
        [SerializeField]
        private bool isFadeOut = true;
        protected override IEffectMaker CreateEffectMaker()
        {
            return new ShakePositionTweenEffect(targeTransform, strength, duration, vibrato, isFadeOut);
        }
    }
}