using DG.Tweening;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class ShakePositionTweenEffect : BaseDotweenEffect
    {
        private Transform targeTransform;
        private Vector3 strength;
        private float duration;
        private int vibrato;
        private bool isFadeOut;

        public ShakePositionTweenEffect(Transform targeTransform, Vector3 strength, float duration, int vibrato, bool isFadeOut)
        {
            this.targeTransform = targeTransform;
            this.strength = strength;
            this.duration = duration;
            this.vibrato = vibrato;
            this.isFadeOut = isFadeOut;
        }


        protected override Tween CreateTween()
        {
            return targeTransform.DOShakePosition(duration, strength, vibrato, fadeOut: isFadeOut);
        }
    }
}