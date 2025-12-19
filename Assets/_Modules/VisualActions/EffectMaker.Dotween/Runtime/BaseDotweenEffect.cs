using DG.Tweening;
using Mimi.EffectMaker.Core;

namespace Mimi.EffectMaker.Dotween
{
    public abstract class BaseDotweenEffect : BaseEffectMaker
    {
        protected Tween tween;
        public override void Initialize()
        {
            
        }

        protected abstract Tween CreateTween();

        public override void StartEffect()
        {
            base.StartEffect();
            
            this.tween = CreateTween();
            this.tween.OnComplete(End).Play();
        }

        public override void End()
        {
            base.End();
            this.tween = null;
        }

        public override bool Pause()
        {
            if(base.Pause() && this.tween != null)
            {
                this.tween.Pause();
                return true;
            }
            return false;
        }

        public override bool Continue()
        {
            if (base.Continue() && this.tween != null)
            {
                this.tween.Play();
                return true;
            }

            return false;
        }

        public override bool Cancel()
        {
            if (base.Cancel() && this.tween != null)
            {
                this.tween.Kill();
                this.tween = null;
                return true;
            }

            return false;
        }
    }
}