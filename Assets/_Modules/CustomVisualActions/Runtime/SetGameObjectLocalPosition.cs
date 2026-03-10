using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class SetGameObjectLocalPosition : VisualAction
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 position;
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.target.localPosition = this.position;
        await UniTask.CompletedTask;
    }
}