using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Actions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mimi.VisualActions.ControlFlow
{
    public class VisualWaitRandomSeconds : VisualAction
    {
        [SerializeField] private Vector2 randomRange;

        private WaitSeconds waitSeconds;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            float seconds = Random.Range(this.randomRange.x, this.randomRange.y);
            this.waitSeconds = new WaitSeconds(seconds);
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