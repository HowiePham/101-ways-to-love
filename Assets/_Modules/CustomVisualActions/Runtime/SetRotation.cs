using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class SetRotation : VisualAction
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 rotation;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.target.rotation = Quaternion.Euler(this.rotation);
        await UniTask.CompletedTask;
    }
}