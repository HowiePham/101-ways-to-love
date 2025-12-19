using System.Threading;
using Cysharp.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    public class SetSpineTimeScale : VisualAction
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private int timeScale;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.skeletonAnimation.timeScale = this.timeScale;
            await UniTask.CompletedTask;
        }
    }
}