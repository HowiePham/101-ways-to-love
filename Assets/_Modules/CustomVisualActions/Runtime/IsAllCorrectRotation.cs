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
                if (rotateObject.eulerAngles.z != 0)
                {
                    Debug.Log($"--- (WAIT) Not Correct Rotation: {rotateObject.gameObject.name}");
                    return false;
                }
            }

            Debug.Log($"--- (WAIT) Correct all rotation");
            return true;
        }
    }
}