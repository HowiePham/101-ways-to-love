using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    public class PlaySpineAnimIncrement : MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private int track;
        [SerializeField] private bool loop;

        [SerializeField, SpineAnimation(dataField = "skeletonAnimation")]
        private string[] animations;


        private int currentIndex = -1;

        public void NextAnimation()
        {
            if (this.currentIndex >= this.animations.Length)
            {
                Debug.LogError(
                    $"Out of range: There are {this.animations.Length} animations but index is {this.currentIndex}");
                return;
            }

            this.currentIndex++;
            this.skeletonAnimation.AnimationState.SetAnimation(this.track, this.animations[this.currentIndex],
                this.loop);
        }
    }
}