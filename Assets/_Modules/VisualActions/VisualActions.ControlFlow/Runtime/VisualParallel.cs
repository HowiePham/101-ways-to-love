using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Actions;
using Sirenix.OdinInspector;

namespace Mimi.VisualActions.ControlFlow
{
    [TypeInfoBox("Execute children all at once.\nExit when all children have completed.")]
    public class VisualParallel : VisualAction
    {
        private Parallel parallel;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            this.parallel = new Parallel();
            IEnumerable<VisualAction> children = GetAllChildActions();

            foreach (VisualAction action in children)
            {
                this.parallel.Add(action);
                await action.Initialize();
            }
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                await this.parallel.Execute(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
        }
    }
}