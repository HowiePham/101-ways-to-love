using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Interactions.Dragging;
using UnityEngine;

namespace Mimi.VisualActions.Dragging
{
    public class WaitDraggableDeselect : VisualAction
    {
        [SerializeField] private BaseDraggable draggable;
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => !draggable.IsSelected, cancellationToken: cancellationToken);
        }
    }
}