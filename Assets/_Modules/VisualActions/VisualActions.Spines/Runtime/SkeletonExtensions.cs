using System.Threading;
using Cysharp.Threading.Tasks;
using Spine.Unity;

namespace Mimi.VisualActions.Spines
{
    public static class SkeletonExtensions
    {
        public static void PlayAnimation(this SkeletonGraphic skeletonGraphic, string animation, bool loop,
            int track = 0)
        {
            PlaySpineAnimation(skeletonGraphic, animation, loop, track);
        }

        public static void PlayAnimation(this SkeletonAnimation skeletonAnimation, string animation, bool loop,
            int track = 0)
        {
            PlaySpineAnimation(skeletonAnimation, animation, loop, track);
        }

        public static async UniTask WaitAnimation(this SkeletonGraphic skeletonGraphic, string animation,
            int track = 0, CancellationToken cancellationToken = default)
        {
            await WaitSpineAnimation(skeletonGraphic, animation, track, cancellationToken);
        }

        public static async UniTask WaitAnimation(this SkeletonAnimation skeletonAnimation, string animation,
            int track = 0, CancellationToken cancellationToken = default)
        {
            await WaitSpineAnimation(skeletonAnimation, animation, track, cancellationToken);
        }

        private static void PlaySpineAnimation(IAnimationStateComponent animationStateComponent, string animation,
            bool loop,
            int track)
        {
            animationStateComponent.AnimationState.SetAnimation(track, animation, loop);
        }

        private static async UniTask WaitSpineAnimation(IAnimationStateComponent animationStateComponent,
            string animation,
            int track, CancellationToken cancellationToken)
        {
            animationStateComponent.AnimationState.SetAnimation(track, animation, false);
            await UniTask.WaitUntil(() => animationStateComponent.AnimationState.GetCurrent(track) == null ||
                                          animationStateComponent.AnimationState.GetCurrent(track).IsComplete,
                cancellationToken: cancellationToken);
        }
    }
}