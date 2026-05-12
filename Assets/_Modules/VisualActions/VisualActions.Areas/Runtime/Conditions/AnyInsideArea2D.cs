using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Dragging
{
    public class AnyInsideArea2D : VisualCondition
    {
        [SerializeField] protected Transform[] checkTransforms;
        [SerializeField] protected BaseArea targetArea;
        [SerializeField] protected bool isDisableCheckedObject;

        public override bool Validate()
        {
            if (!this.targetArea.Active) return false;

            foreach (Transform checkTransform in this.checkTransforms)
            {
                if (!checkTransform.gameObject.activeSelf)
                {
                    continue;
                }

                Vector3 checkPos = checkTransform.position;

                if (!this.targetArea.ContainsWorldSpace(checkPos))
                {
                    continue;
                }

                if (this.isDisableCheckedObject)
                {
                    checkTransform.gameObject.SetActive(false);
                }

                return true;
            }

            return false;
        }
    }
}