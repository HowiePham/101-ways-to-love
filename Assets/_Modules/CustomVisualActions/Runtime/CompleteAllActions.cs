using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

namespace _Modules.VisualFlow.Mechanics.FindSortMatch
{
    public class CompleteAllActions : VisualAction
    {
        [SerializeField] private List<VisualAction> actions;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            foreach (var action in this.actions)
            {
                
            }
        }
    }
}