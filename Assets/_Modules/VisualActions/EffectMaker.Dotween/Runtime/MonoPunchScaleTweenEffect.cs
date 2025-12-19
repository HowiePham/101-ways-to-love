using DG.Tweening;
using Mimi.EffectMaker.Core;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class MonoPunchScaleTweenEffect: MonoEffectMaker
    {
        [MainInput]
        [SerializeField]
        private Transform target;
        [SerializeField]
        private Vector3 strength;
        [SerializeField]
        private float duration;
        [SerializeField]
        private Ease ease;


        protected override IEffectMaker CreateEffectMaker()
        {
            return new PunchScaleTweenEffect(target, strength, duration, ease);
        }
    }
}