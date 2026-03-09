using Mimi.VisualActions;
using UnityEngine;

namespace _Modules.VisualFlow.Mechanics.FindSortMatch
{
    public class IsAllCorrectRotation : VisualCondition
    {
        [SerializeField] private Transform[] rotateObjects;
        [SerializeField] private Vector3 targetRotation;

        public override bool Validate()
        {
            foreach (Transform rotateObject in this.rotateObjects)
            {
                if (!Mathf.Approximately(rotateObject.eulerAngles.z, this.targetRotation.z))
                {
                    return false;
                }
            }

            return true;
        }
    }
}