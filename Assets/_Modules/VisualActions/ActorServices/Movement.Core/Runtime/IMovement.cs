using UnityEngine;

namespace Mimi.Actor.Movement.Core
{
    public interface IMovement
    {
        public void SetPosition(Vector3 position);
        public Vector3 GetPosition();
    }
}