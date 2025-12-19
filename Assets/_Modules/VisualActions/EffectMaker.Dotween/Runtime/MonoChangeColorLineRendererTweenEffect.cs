using DG.Tweening;
using Mimi.EffectMaker.Core;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class MonoChangeColorLineRendererTweenEffect : MonoEffectMaker
    {
        [SerializeField]
        private LineRenderer _lineRenderer;
        [SerializeField]
        private Color startColor = Color.white;
        [SerializeField]
        private Color targetColor;
        [SerializeField]
        private float duration = 0.3f;
        [SerializeField]
        private Ease ease;
        protected override IEffectMaker CreateEffectMaker()
        {
            return new ChangeColorLineRendererTweenEffect(_lineRenderer, startColor, targetColor, duration, ease);
        }
    }
}