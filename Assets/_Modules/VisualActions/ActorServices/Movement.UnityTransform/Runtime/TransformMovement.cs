using Mimi.Actor.Movement.Core;
using UnityEngine;

namespace Mimi.Actor.Movement.Transform
{
    public class TransformMovement : IMovement
    {
        private UnityEngine.Transform _transform;

        public TransformMovement(UnityEngine.Transform transform)
        {
            _transform = transform;
        }
        public void SetPosition(Vector3 position)
        {
            _transform.position = position;
        }

        public Vector3 GetPosition()
        {
            return _transform.position;
        }
    }
}