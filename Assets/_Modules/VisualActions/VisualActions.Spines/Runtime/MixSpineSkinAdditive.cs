using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    public class MixSpineSkinAdditive : VisualAction
    {
        [SerializeField, Required] private MonoSpineMixSkin mixSkin;

#if UNITY_EDITOR
        [SerializeField, Required] private SkeletonAnimation skeletonAnimation;
#endif

        [SerializeField, SpineSkin(dataField = "skeletonAnimation")]
        private string skinName;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.mixSkin.SetSlot(this.skinName);
            await UniTask.CompletedTask;
        }
    }
}