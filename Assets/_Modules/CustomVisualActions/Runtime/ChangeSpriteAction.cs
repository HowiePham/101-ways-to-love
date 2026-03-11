using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class ChangeSpriteAction : VisualAction
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite sprite;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.spriteRenderer.sprite = this.sprite;
        await UniTask.CompletedTask;
    }
}