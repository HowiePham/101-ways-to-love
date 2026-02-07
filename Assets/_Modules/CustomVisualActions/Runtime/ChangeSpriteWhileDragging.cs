using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

public class ChangeSpriteWhileDragging : MonoDraggableExtension
{
    [SerializeField] private Sprite swappedSprite;
    [SerializeField] private SpriteRenderer itemSprite;
    private Sprite oldSprite;

    public override void StartDrag()
    {
        this.oldSprite = this.itemSprite.sprite;
    }

    public override void Drag()
    {
        this.itemSprite.sprite = this.swappedSprite;
    }

    public override void EndDrag()
    {
        this.itemSprite.sprite = this.oldSprite;
    }
}