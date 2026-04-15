using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

public class SpriteSwapWhileDragging : MonoDraggableExtension
{
    [SerializeField] private Sprite swappedSprite;
    [SerializeField] private SpriteRenderer itemSprite;
    [SerializeField] private float changeDelay;
    [SerializeField] private float rollbackDelay;

    private Sprite oldSprite;
    private CancellationTokenSource cts;

    public override void Init(BaseDraggable draggable)
    {
        base.Init(draggable);

        if (this.itemSprite != null)
        {
            this.oldSprite = this.itemSprite.sprite;
        }
    }

    public override void StartDrag()
    {
        ResetCts();
        ChangeSprite(this.swappedSprite, this.changeDelay, this.cts.Token).Forget();
    }

    public override void Drag()
    {
    }

    public override void EndDrag()
    {
        ResetCts();
        ChangeSprite(this.oldSprite, this.rollbackDelay, this.cts.Token).Forget();
    }

    private void OnDestroy()
    {
        this.cts?.Cancel();
        this.cts?.Dispose();
    }

    private void ResetCts()
    {
        this.cts?.Cancel();
        this.cts?.Dispose();
        this.cts = new CancellationTokenSource();
    }

    private async UniTask ChangeSprite(Sprite newSprite, float delay, CancellationToken cancellationToken)
    {
        if (this.itemSprite != null)
        {
            await UniTask.WaitForSeconds(delay, cancellationToken: cancellationToken);
            this.itemSprite.sprite = newSprite;
        }
    }
}