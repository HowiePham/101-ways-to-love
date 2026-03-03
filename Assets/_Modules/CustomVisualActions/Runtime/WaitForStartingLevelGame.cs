using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.Events;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;

public class WaitForStartingLevelGame : VisualAction
{
    [SerializeField] private bool waitForStartingLevel;
    private bool isComplete;

    protected override UniTask OnEnter(CancellationToken cancellationToken)
    {
        this.isComplete = false;
        Messenger.AddListener(EventKey.StartLevelGame, StartLevelGameHandler);
        return base.OnEnter(cancellationToken);
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (!this.waitForStartingLevel)
        {
            StartLevelGameHandler();
        }
        else
        {
            Messenger.Broadcast(EventKey.ShowStartLevelGameButton);
        }

        await UniTask.WaitUntil(() => this.isComplete, cancellationToken: cancellationToken);
    }

    protected override UniTask OnExit(CancellationToken cancellationToken)
    {
        Messenger.RemoveListener(EventKey.StartLevelGame, StartLevelGameHandler);
        return base.OnExit(cancellationToken);
    }

    [Button]
    private void StartLevelGameHandler()
    {
        this.isComplete = true;
    }
}