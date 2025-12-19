using System;

namespace Mimi.EffectMaker.Core.Decorators
{
    public abstract class BaseEffectMakerDecorator : IEffectMaker
    {
        public IEffectMaker WrapEffectMaker { get; private set; }

        public BaseEffectMakerDecorator(IEffectMaker wrapEffectMaker)
        {
            this.WrapEffectMaker = wrapEffectMaker;
        }
        public event Action OnEffectStart;
        public event Action OnEffectEnd;
        public event Action OnEffectPaused;
        public event Action OnEffectContinued;
        public bool IsPlaying => WrapEffectMaker.IsPlaying;
        public bool IsCompleted => WrapEffectMaker.IsCompleted;
        public bool IsPaused => WrapEffectMaker.IsPaused;
        public void Initialize()
        {
            WrapEffectMaker.OnEffectStart += () => OnEffectStart?.Invoke();
            WrapEffectMaker.OnEffectEnd += End;
            WrapEffectMaker.OnEffectPaused += () => OnEffectPaused?.Invoke();
            WrapEffectMaker.OnEffectContinued += () => OnEffectContinued?.Invoke();
            WrapEffectMaker.Initialize();
        }

        
        public virtual void StartEffect()
        {
            WrapEffectMaker.StartEffect();
        }

        public virtual void End()
        {
            OnEffectEnd?.Invoke();
        }

        public virtual bool Cancel()
        {
            return WrapEffectMaker.Cancel();
        }

        public virtual bool Pause()
        {
            return WrapEffectMaker.Pause();
        }

        public virtual bool Continue()
        {
            return WrapEffectMaker.Continue();
        }
    }
}