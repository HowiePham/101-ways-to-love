using Mimi.VisualActions;
using UnityEngine;

namespace _Modules.VisualFlow.Mechanics.FindSortMatch
{
    public class IsCorrectRotation : VisualCondition
    {
        [SerializeField] private Transform rotateObject;
        [SerializeField] private Vector3[] targetRotation;
        private float tolerance = 0.1f;

        public override bool Validate()
        {
            Vector3 current = this.rotateObject.eulerAngles;

            foreach (Vector3 rotation in this.targetRotation)
            {
                bool isCorrect =
                    Mathf.Abs(Mathf.DeltaAngle(current.x, rotation.x)) <= this.tolerance &&
                    Mathf.Abs(Mathf.DeltaAngle(current.y, rotation.y)) <= this.tolerance &&
                    Mathf.Abs(Mathf.DeltaAngle(current.z, rotation.z)) <= this.tolerance;

                if (isCorrect)
                {
                    return true;
                }
            }

            return false;
        }
    }
}