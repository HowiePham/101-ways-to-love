using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mimi.VisualActions
{
    public class WaitAllConditions : VisualAction
    {
        [SerializeField] private VisualCondition[] conditions;
        private bool complete;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => this.complete, cancellationToken: cancellationToken);
        }

        private void Update()
        {
            if (this.complete)
            {
                return;
            }

            foreach (VisualCondition condition in this.conditions)
            {
                if (!condition.Validate())
                {
                    this.complete = false;
                    return;
                }
            }

            this.complete = true;
        }
    }
}