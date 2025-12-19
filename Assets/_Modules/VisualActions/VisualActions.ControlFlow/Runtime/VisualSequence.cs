using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Actions;
using Sirenix.OdinInspector;

namespace Mimi.VisualActions.ControlFlow
{
    [TypeInfoBox("Execute children from top to bottom.\nExit when the last child has completed.")]
    public class VisualSequence : VisualAction
    {
        private Sequence sequence;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            this.sequence = new Sequence();
            IEnumerable<VisualAction> children = GetAllChildActions();

            foreach (VisualAction action in children)
            {
                this.sequence.Add(action);
                await action.Initialize();
            }
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                await this.sequence.Execute(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
        }
    }
}