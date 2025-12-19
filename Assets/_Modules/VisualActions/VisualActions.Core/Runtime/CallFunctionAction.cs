using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Mimi.VisualActions
{
    public class CallFunctionAction : VisualAction
    {
        [SerializeField] private UnityEvent action;
        protected override UniTask OnExecuting(CancellationToken cancellationToken)
        {
            action?.Invoke();
            return UniTask.CompletedTask;
        }
    }
}