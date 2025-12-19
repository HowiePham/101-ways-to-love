namespace Mimi.EffectMaker.Core
{
    public class ParallelEffectMaker : BaseEffectMaker
    {
        private IEffectMaker[] effects;
        private int numberOfEffectCompleted;

        public ParallelEffectMaker(IEffectMaker[] effects)
        {
            this.effects = effects;
        }
        public override void Initialize()
        {
            foreach (var effect in effects)
            {
                effect.Initialize();
            }
        }

        public override void StartEffect()
        {
            base.StartEffect();
            numberOfEffectCompleted = 0;
            foreach (var effect in effects)
            {
                effect.OnEffectEnd += OnEffectCompleted;
                effect.StartEffect();
            }
        }

       
        public override void End()
        {
            base.End();
            foreach (var effect in effects)
            {
                effect.OnEffectEnd -= OnEffectCompleted;
            }
        }

        public override bool Cancel()
        {
            if (!base.Cancel()) return false;
            foreach (var effect in effects)
            {
                effect.Cancel();
            }
            return true;
        }

        public override bool Pause()
        {
            if (!base.Pause()) return false;
            foreach (var effect in effects)
            {
                effect.Pause();
            }

            return true;
        }

        public override bool Continue()
        {
            if (!base.Continue()) return false;
            foreach (var effect in effects)
            {
                effect.Continue();
            }

            return true;
        }

        private void OnEffectCompleted()
        {
            numberOfEffectCompleted++;
            if (numberOfEffectCompleted >= effects.Length)
            {
                End();
            }
        }

    }
}