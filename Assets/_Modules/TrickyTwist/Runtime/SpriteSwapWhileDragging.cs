using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

public class SpriteSwapWhileDragging : MonoDraggableExtension
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite swappedSprite;
    private Sprite originSprite;

    public override void Init(BaseDraggable draggable)
    {
        base.Init(draggable);

        if (this.spriteRenderer == null)
        {
            return;
        }

        this.originSprite = this.spriteRenderer.sprite;
    }

    public override void StartDrag()
    {
        if (this.spriteRenderer == null || this.swappedSprite == null)
        {
            return;
        }

        this.spriteRenderer.sprite = this.swappedSprite;
    }

    public override void Drag()
    {
    }

    public override void EndDrag()
    {
        if (this.spriteRenderer == null)
        {
            return;
        }

        this.spriteRenderer.sprite = this.originSprite;
    }
}