using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Actions;
using Sirenix.OdinInspector;

namespace Mimi.VisualActions.ControlFlow
{
    [TypeInfoBox("Execute children from top to bottom.\nExit when any child has completed.")]
    public class VisualWaitAny : VisualAction
    {
        private WaitAny waitAny;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            this.waitAny = new WaitAny();
            IEnumerable<VisualAction> children = GetAllChildActions();

            foreach (VisualAction action in children)
            {
                this.waitAny.Add(action);
                await action.Initialize();
            }
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                await this.waitAny.Execute(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
        }
    }
}