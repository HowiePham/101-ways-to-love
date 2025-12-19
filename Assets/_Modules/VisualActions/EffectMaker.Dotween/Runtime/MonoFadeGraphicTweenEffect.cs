using DG.Tweening;
using Mimi.Actor.Graphic.Core;
using Mimi.EffectMaker.Core;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class MonoFadeGraphicTweenEffect : MonoEffectMaker
    {
        [MainInput]
        [SerializeField] private BaseMonoGraphic graphic;
        [SerializeField,Range(0,1f)]
        private float startAlpha;
        [SerializeField,Range(0,1f)]
        private float endAlpha;
        [SerializeField]
        private float duration;
        [SerializeField]
        private Ease ease;
        protected override IEffectMaker CreateEffectMaker()
        {
            return new FadeGraphicTweenEffect(graphic,  startAlpha, endAlpha, duration, ease);
        }
    }
}