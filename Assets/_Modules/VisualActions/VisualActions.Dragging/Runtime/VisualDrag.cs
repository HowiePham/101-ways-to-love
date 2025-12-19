using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Interactions.Dragging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mimi.VisualActions.Dragging
{
    [TypeInfoBox("Drag Handler: Handles user input.\nComplete Condition: Condition for completing the drag action.\nDrag Extensions: Extends drag behavior.\nisCheckCompleteWhenDrag: Check completion during drag (default: only on release).")]
    public class VisualDrag : VisualAction
    {
        [SerializeField] private BaseDragHandler dragHandler;
        [SerializeField] private VisualCondition completeCondition;
        [SerializeField] private MonoDragExtension dragExtension;
        [SerializeField] private bool isCheckCompleteWhenDrag;

        private DragInteraction dragInteraction;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            if (this.dragExtension == null)
            {
                this.dragExtension = gameObject.AddComponent<NullMonoDragExtension>();
            }

            if (this.dragHandler == null)
            {
                this.dragHandler = FindFirstObjectByType<BaseDragHandler>();
            }

            this.dragInteraction = new DragInteraction(this.dragHandler, this.completeCondition, this.dragExtension,isCheckCompleteWhenDrag);
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.dragInteraction.Start();

            try
            {
                await UniTask.WaitUntil(() => this.dragInteraction.IsCompleted, PlayerLoopTiming.Update,
                    cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
            finally
            {
                this.dragInteraction.Stop();
            }
        }
    }
}