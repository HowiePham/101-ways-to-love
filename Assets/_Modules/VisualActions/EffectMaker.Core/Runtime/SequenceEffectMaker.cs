namespace Mimi.EffectMaker.Core
{
    public class SequenceEffectMaker : BaseEffectMaker
    {
        private IEffectMaker[] effects;
        private int currentIndex;
        public SequenceEffectMaker(IEffectMaker[] arrays)
        {
            this.effects = arrays;
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
            this.currentIndex = 0;
            PlayCurrentIndex();
        }

        public override bool Pause()
        {
            if (!base.Pause()) return false;
            effects[currentIndex].Pause();
            return true;
        }

        public override bool Continue()
        {
            if (!base.Continue()) return false;
            effects[currentIndex].Continue();
            return true;
        }

        public override bool Cancel()
        {
            if (!base.Cancel()) return false;
            effects[currentIndex].Cancel();
            effects[currentIndex].OnEffectEnd -= CurrentEffectEnd;
            return true;
        }

        private void PlayCurrentIndex()
        {
            effects[currentIndex].OnEffectEnd += CurrentEffectEnd;
            effects[currentIndex].StartEffect();
        }

        private void CurrentEffectEnd()
        {
            effects[currentIndex].OnEffectEnd -= CurrentEffectEnd;
            currentIndex++;
            if (currentIndex > this.effects.Length - 1)
            {
                End();
                return;
            }
            PlayCurrentIndex();
        }
    }
}