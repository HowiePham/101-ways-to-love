using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class ChangeOrderInLayer : VisualAction
{
    [SerializeField] private Renderer renderer;
    [SerializeField] private int orderInLayer;


    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.renderer.sortingOrder = this.orderInLayer;
        await UniTask.CompletedTask;
    }
}