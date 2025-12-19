using DG.Tweening;
using Mimi.EffectMaker.Core;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class MonoScaleLineRendererTweenEffect : MonoEffectMaker
    {
        [SerializeField]
        private LineRenderer target;
        [SerializeField]
        private float initialWidth;
        [SerializeField] private float targetWidth;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private Ease ease;
        protected override IEffectMaker CreateEffectMaker()
        {
            return new ScaleLineRendererTweenEffect(target, initialWidth, targetWidth, duration, ease);
        }
    }
}