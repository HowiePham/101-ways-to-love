using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class SetGameObjectPosition : VisualAction
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 position;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.target.position = this.position;
        await UniTask.CompletedTask;
    }
}