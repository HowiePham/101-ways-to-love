using System;
using UnityEngine;

namespace Mimi.Actor.ObjectInputReceiver.Core
{
    public interface IObjectInputReceiver 
    {
        public event Action<Vector3> OnSelect;
        public event Action<Vector3> OnDrag;
        public event Action<Vector3> OnRelease;
        public event Action<Vector3> OnTap;

        public void Initialize();
        public void Dispose();
    }
}