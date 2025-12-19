using Mimi.Actor.Graphic.Core;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class ItemChangeSpriteExtension : MonoItemDragExtension
    {
        [SerializeField] private MonoGraphicAsset targetSprite;

        private MonoGraphicAsset initialSprite;
        public override void OnInit(DraggableItem draggableItem)
        {
            this.initialSprite = (MonoGraphicAsset)draggableItem.Graphic.CurrentGraphicAsset;
        }

        public override void OnPickUp(DraggableItem draggableItem)
        {
            draggableItem.Graphic.SetAssetGraphic(this.targetSprite);
        }

        public override void OnReturn(DraggableItem draggableItem)
        {
            draggableItem.Graphic.SetAssetGraphic(this.initialSprite);
        }
    }
}