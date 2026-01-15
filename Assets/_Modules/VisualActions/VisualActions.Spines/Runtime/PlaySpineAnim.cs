using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    [TypeInfoBox("Play an animation fire and forget.")]
    public class PlaySpineAnim : SpineAnimMechanic
    {
        [SerializeField] private int track;
        [SerializeField] private bool loop;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (!HasAnimation())
            {
                await UniTask.CompletedTask;
                return;
            }
            
            this.SkeletonAnimation.timeScale = this.timeScale;
            this.SkeletonAnimation.AnimationState.SetAnimation(this.track, this.Animation, this.loop);
            await UniTask.CompletedTask;
        }
    }
}