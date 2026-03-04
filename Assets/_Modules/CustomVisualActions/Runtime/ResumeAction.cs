using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class ResumeAction : VisualAction
{
    [SerializeField] private VisualAction action;
    
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.action.Resume();
    }
}