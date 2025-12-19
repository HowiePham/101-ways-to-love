using Mimi.EffectMaker.Core;
using UnityEngine;
using MonoEffectMaker = Mimi.EffectMaker.Core.MonoEffectMaker;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class ItemPlayEffectWhenPickUp : MonoItemDragExtension
    {
        [SerializeField] private MonoEffectMaker effectMaker;
        public override void OnInit(DraggableItem draggableItem)
        {
            effectMaker.Initialize();
        }

        public override void OnPickUp(DraggableItem draggableItem)
        {
            effectMaker.StartEffect();
        }

        public override void OnReturn(DraggableItem draggableItem)
        {
            
        }
    }
}