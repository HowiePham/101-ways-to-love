using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.EffectMaker.Core;
using Mimi.VisualActions;
using UnityEngine;

public class RunMonoEffect : VisualAction
{
    [SerializeField] private MonoEffectMaker monoEffectMaker;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.monoEffectMaker.StartEffect();
        await UniTask.CompletedTask;
    }
}