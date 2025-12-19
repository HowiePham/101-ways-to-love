using Spine;
using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    public class IKSpine : MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        [SerializeField, SpineBone(dataField = "skeletonAnimation")]
        private string ikBoneName;

        [SerializeField] private Transform target;

        private Transform skeletonTrans;
        private Bone targetBone;
        private bool active;

        private void Start()
        {
            this.targetBone = this.skeletonAnimation.skeleton.FindBone(this.ikBoneName);
            this.skeletonTrans = this.skeletonAnimation.transform;
        }

        private void OnEnable()
        {
            this.skeletonAnimation.UpdateLocal += UpdateTargetPosition;
        }

        private void OnDisable()
        {
            this.skeletonAnimation.UpdateLocal -= UpdateTargetPosition;
        }

        private void UpdateTargetPosition(ISkeletonAnimation animated)
        {
            if (!this.active) return;
            Vector3 localPosition = this.skeletonTrans.InverseTransformPoint(this.target.position);
            this.targetBone.SetLocalPosition(localPosition);
        }

        public void SetIKActive(bool active)
        {
            this.active = active;
        }
    }
}