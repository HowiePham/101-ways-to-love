using DG.Tweening;
using Mimi.EffectMaker.Core;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class MonoScaleTweenEffect : MonoEffectMaker
    {
        [MainInput]
        [SerializeField]
        private Transform target;
        [SerializeField]
        private Vector3 initialScale;
        [SerializeField]
        private Vector3 targetScale;
        [SerializeField]
        private float duration;
        [SerializeField]
        private Ease ease;


        protected override IEffectMaker CreateEffectMaker()
        {
            return new ScaleTweenEffect(target, initialScale, targetScale, duration, ease);
        }
    }
}