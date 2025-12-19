using System;

namespace Mimi.EffectMaker.Core
{
    public interface IEffectMaker
    {
        public event Action OnEffectStart;
        public event Action OnEffectEnd;
        public event Action OnEffectPaused;
        public event Action OnEffectContinued;
        public bool IsPlaying { get; }
        public bool IsCompleted { get;}
        public bool IsPaused { get; }
        public void Initialize();
        public void StartEffect();
        public void End();
        public bool Cancel();
        public bool Pause();
        public bool Continue();
    }
}