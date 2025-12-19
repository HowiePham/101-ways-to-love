using UnityEngine;

namespace Mimi.Actor.Core
{
    public abstract class MonoActorAspect : MonoBehaviour, IActorAspect
    {
        public abstract void Initialize();

        public abstract void Dispose();
    }
}