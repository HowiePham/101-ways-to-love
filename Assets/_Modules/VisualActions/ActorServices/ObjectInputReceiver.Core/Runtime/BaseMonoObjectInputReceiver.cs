using System;
using Mimi.Actor.Core;
using UnityEngine;

namespace Mimi.Actor.ObjectInputReceiver.Core
{
    public abstract class BaseMonoObjectInputReceiver: MonoActorAspect, IObjectInputReceiver
    {
        public event Action<Vector3> OnSelect;
        public event Action<Vector3> OnDrag;
        public event Action<Vector3> OnRelease;
        public event Action<Vector3> OnTap;
        
        private IObjectInputReceiver objectInputReceiver;

        public IObjectInputReceiver WrapObjectInputReceiver
        {
            get
            {
                if (objectInputReceiver == null)
                {
                    Initialize();
                }
                return objectInputReceiver;
            }
        }
        protected abstract IObjectInputReceiver CreateObjectInputReceiver();
        public override void Initialize()
        {
           objectInputReceiver = CreateObjectInputReceiver();
           objectInputReceiver.Initialize();
           objectInputReceiver.OnSelect += OnItemSelect;
           objectInputReceiver.OnDrag += OnItemDrag;
           objectInputReceiver.OnRelease += OnItemRelease;
           objectInputReceiver.OnTap += OnItemTap;
        }

        void OnItemSelect(Vector3 position)
        {
            OnSelect?.Invoke(position);
        }

        void OnItemDrag(Vector3 position)
        {
            OnDrag?.Invoke(position);
        }

        void OnItemRelease(Vector3 position)
        {
            OnRelease?.Invoke(position);
        }

        void OnItemTap(Vector3 position)
        {
            OnTap?.Invoke(position);
        }
        public override void Dispose()
        {
            
            objectInputReceiver.OnSelect -= OnItemSelect;
            objectInputReceiver.OnDrag -= OnItemDrag;
            objectInputReceiver.OnRelease -= OnItemRelease;
            objectInputReceiver.OnTap -= OnItemTap;
            objectInputReceiver.Dispose();
        }
    }
}