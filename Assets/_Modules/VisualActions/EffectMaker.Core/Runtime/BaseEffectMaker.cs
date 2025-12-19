using System;

namespace Mimi.EffectMaker.Core
{
    public abstract class BaseEffectMaker : IEffectMaker
    {
        public event Action OnEffectStart;
        public event Action OnEffectEnd;
        public event Action OnEffectPaused;
        public event Action OnEffectContinued;
        public bool IsPlaying { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsPaused { get; private set; }
        public abstract void Initialize();

        public virtual void StartEffect()
        {
            Cancel();
            IsPlaying = true;
            IsCompleted = false;
            IsPaused = false;
        }

        public virtual void End()
        {
            IsPlaying = false;
            IsCompleted = true;
            IsPaused = false;
            OnEffectEnd?.Invoke();
        }

        public virtual bool Cancel()
        {
            if (!IsPlaying) return false;
            IsPlaying = false;
            IsCompleted = true;
            IsPaused = false;
            OnEffectEnd?.Invoke();
            return true;
        }

        public virtual bool Pause()
        {
            if (!IsPlaying) return false;
            IsPaused = true;
            IsPlaying = false;
            OnEffectPaused?.Invoke();
            return true;
        }

        public virtual bool Continue()
        {
            if (!IsPaused) return false;
            IsPaused = false;
            IsPlaying = true;
            OnEffectPaused?.Invoke();
            return true;
        }
    }
}