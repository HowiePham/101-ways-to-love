using UnityEngine;

namespace Mimi.Actor.Movement.Core
{
    public abstract class MonoMovement : MonoBehaviour, IMovement
    {
        private IMovement wrapperMovement;

        private IMovement WrapperMovement
        {
            get
            {
                if (wrapperMovement == null)
                {
                    wrapperMovement = CreateMovement();
                }

                return wrapperMovement;
            }
        }
        protected abstract IMovement CreateMovement();
        public void SetPosition(Vector3 position)
        {
            WrapperMovement.SetPosition(position);
        }

        public Vector3 GetPosition()
        {
            return WrapperMovement.GetPosition();
        }
    }
}