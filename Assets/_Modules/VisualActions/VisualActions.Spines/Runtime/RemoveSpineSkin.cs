using System.Threading;
using Cysharp.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    public class RemoveSpineSkin : VisualAction
    {
        [SerializeField] private MonoSpineMixSkin mixSkin;
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        [SerializeField, SpineSkin(dataField = "skeletonAnimation")]
        private string skin;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.mixSkin.RemoveSkin(this.skin);
            await UniTask.CompletedTask;
        }
    }
}