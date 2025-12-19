using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.EffectMaker.Core
{
    public abstract class MonoEffectMaker : MonoBehaviour, IEffectMaker
    {
        public event Action OnEffectStart;
        public event Action OnEffectEnd;
        public event Action OnEffectPaused;
        public event Action OnEffectContinued;
        [ShowInInspector] public bool IsPlaying
        {
            get
            {
                if (wrapEffectMaker == null) return false;
                return wrapEffectMaker.IsPlaying;
            }
        }

        [ShowInInspector] public bool IsCompleted
        {
            get
            {
                if (wrapEffectMaker == null) return false;
                return wrapEffectMaker.IsCompleted;
            }
        }

        [ShowInInspector] public bool IsPaused
        {
            get
            {
                if (wrapEffectMaker == null) return false;
                return wrapEffectMaker.IsPaused;
            }
        }

        private bool isInitalized = false;
        protected virtual void Awake()
        {
            Initialize();
        }
        public void Initialize()
        {
            if(isInitalized) return;
            wrapEffectMaker = CreateEffectMaker();
            wrapEffectMaker.OnEffectStart += () => OnEffectStart?.Invoke();
            wrapEffectMaker.OnEffectEnd += End;
            wrapEffectMaker.OnEffectPaused += () => OnEffectPaused?.Invoke();
            wrapEffectMaker.OnEffectContinued += () => OnEffectContinued?.Invoke();
            wrapEffectMaker.Initialize();
            isInitalized = true;
        }

        protected IEffectMaker wrapEffectMaker;
        protected abstract IEffectMaker CreateEffectMaker();

        public void StartEffect()
        {
            wrapEffectMaker.StartEffect();
        }

        public void End()
        {
            OnEffectEnd?.Invoke();
        }

        public bool Cancel()
        {
            return wrapEffectMaker.Cancel();
        }

        public bool Pause()
        {
            return wrapEffectMaker.Pause();
        }

        public bool Continue()
        {
            return wrapEffectMaker.Continue();
        }

        
        #if UNITY_EDITOR
        [Button]
        public void TestEffect()
        {
            StartEffect();
        }

        [Button]
        public void ResetEffect()
        {
            isInitalized = false;
            Initialize();
        }
        #endif
    }
}