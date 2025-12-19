using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    public class MonoSpineMixSkin : MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        private SpineMixSkin spineMixSkin;

        private void Awake()
        {
            this.spineMixSkin = new SpineMixSkin(this.skeletonAnimation);
        }

        public void SetSlot(string skinName)
        {
            this.spineMixSkin.SetSlot(skinName);
        }

        public void RemoveSkin(string skinName)
        {
            this.spineMixSkin.RemoveSkin(skinName);
        }
    }
}