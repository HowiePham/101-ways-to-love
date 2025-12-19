using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mimi.Actor.Core
{
    public abstract class BaseActor : MonoBehaviour
    {
        [SerializeField] private List<MonoActorAspect> actorAspects;
        [SerializeField] private bool isAutoInitialize= true;

        private void Awake()
        {
            if (isAutoInitialize)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public virtual void Initialize()
        {
            foreach (var actorAspect in actorAspects)
            {
                actorAspect.Initialize();
            }
            OnInitialize();
        }

        protected virtual void Dispose()
        {
            foreach (var actorAspect in actorAspects)
            {
                actorAspect.Dispose();
            }
            OnDispose();
        }

        protected abstract UniTask OnDispose();
        protected abstract UniTask OnInitialize();

        public T GetAspect<T>() where T : MonoActorAspect
        {
            foreach (var aspect in actorAspects)
            {
                if (aspect is T)
                {
                    return (T)aspect;
                }
            }

            return null;
        }

        public void SetAspect<T>(T aspect) where T : MonoActorAspect
        {
            var targetAspect = GetAspect<T>();
            if (targetAspect != null)
            {
                targetAspect = aspect;
            }
        }
        
        public void AddAspect<T>(T aspect) where T : MonoActorAspect
        {
            if (aspect == null) return;
            actorAspects.Add(aspect);
        }

        public bool RemoveAspect<T>() where T : MonoActorAspect
        {
            var aspect = GetAspect<T>();
            if (aspect != null)
            {
                return actorAspects.Remove(aspect);
            }

            return false;
        }
    }
}