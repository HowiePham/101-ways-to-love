using DG.Tweening;
using UnityEngine;

namespace Mimi.EffectMaker.Dotween
{
    public class ShakeRotationTweenEffect: BaseDotweenEffect
    {
        private Transform targeTransform;
        private Vector3 strength;
        private float duration;
        private int vibrato;
        private bool isFadeOut;

        public ShakeRotationTweenEffect(Transform targeTransform, Vector3 strength, float duration, int vibrato, bool isFadeOut)
        {
            this.targeTransform = targeTransform;
            this.strength = strength;
            this.duration = duration;
            this.vibrato = vibrato;
            this.isFadeOut = isFadeOut;
        }


        protected override Tween CreateTween()
        {
            return targeTransform.DOShakeRotation(duration, strength, vibrato, fadeOut: isFadeOut);
        }
    }
}