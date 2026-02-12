using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

namespace VisualFlow
{
    public class ClearEraseBrush : VisualAction
    {
        [SerializeField] private Eraser eraser;
        
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (this.eraser != null) this.eraser.Clear();
            await UniTask.CompletedTask;
        }
    }
}