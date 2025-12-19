using Mimi.Actor.Movement.Core;
using UnityEngine;

namespace Mimi.Actor.Movement.Transform
{
    public class MonoTransformMovement : MonoMovement
    {
        [SerializeField] private UnityEngine.Transform tf;
        protected override IMovement CreateMovement()
        {
            return new TransformMovement(tf);
        }
    }
}