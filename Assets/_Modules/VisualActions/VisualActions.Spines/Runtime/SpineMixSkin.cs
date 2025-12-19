using System.Collections.Generic;
using Spine;
using Spine.Unity;

namespace Mimi.VisualActions.Spines
{
    public class SpineMixSkin
    {
        private readonly SkeletonAnimation skeletonAnimation;
        private readonly Skin mixedSkin = new("Mix");

        public SpineMixSkin(SkeletonAnimation skeletonAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
        }

        public void SetSlot(string skinName)
        {
            Skin skin = this.skeletonAnimation.Skeleton.Data.FindSkin(skinName);
            this.mixedSkin.AddSkin(skin);
            this.skeletonAnimation.Skeleton.SetSkin(this.mixedSkin);
            this.skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            this.skeletonAnimation.AnimationState.Apply(this.skeletonAnimation.Skeleton);
        }

        public void RemoveSkin(string skinName)
        {
            Skin skin = this.skeletonAnimation.Skeleton.Data.FindSkin(skinName);
            /*
            foreach (Skin.SkinEntry attachment in skin.Attachments)
            {
                this.mixedSkin.RemoveAttachment(attachment.SlotIndex, attachment.Name);
            }*/

            this.skeletonAnimation.Skeleton.SetSkin(this.mixedSkin);
            this.skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            this.skeletonAnimation.AnimationState.Apply(this.skeletonAnimation.Skeleton);
        }
    }
}