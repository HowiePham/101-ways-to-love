using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Interactions.Dragging;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.VisualActions.Dragging
{
    public class SetInteractableDraggableAction : VisualAction
    {
        [MainInput]
        [SerializeField] private BaseDraggable draggable;

        [SerializeField] private bool status;
        protected override UniTask OnExecuting(CancellationToken cancellationToken)
        {
            draggable.SetInteractable(status);
            return UniTask.CompletedTask;
        }
    }
}