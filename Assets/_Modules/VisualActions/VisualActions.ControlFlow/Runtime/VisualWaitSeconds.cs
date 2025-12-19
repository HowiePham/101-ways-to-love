using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Actions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.ControlFlow
{
    [TypeInfoBox("Wait for seconds.")]
    public class VisualWaitSeconds : VisualAction
    {
        [SerializeField] private float seconds;

        private WaitSeconds waitSeconds;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            this.waitSeconds = new WaitSeconds(this.seconds);
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                await this.waitSeconds.Execute(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
        }
    }
}