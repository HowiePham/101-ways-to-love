using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.EffectMaker.Core;
using UnityEngine;

namespace Mimi.VisualActions.EffectMaker
{
    public class VisualEffectMaker : VisualAction
    {
        [SerializeField] private MonoEffectMaker effectMaker;
        [SerializeField] private bool isWaiting;
        protected override UniTask OnInitializing()
        {
            effectMaker.Initialize();
            return base.OnInitializing();
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            effectMaker.StartEffect();
            if (isWaiting)
            {
                await UniTask.WaitUntil(() => effectMaker.IsCompleted, cancellationToken: cancellationToken);
            }
        }
    }
}