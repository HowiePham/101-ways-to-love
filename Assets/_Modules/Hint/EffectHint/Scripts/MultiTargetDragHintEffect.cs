using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;
using VisualFlow;

public class MultiTargetDragHintEffect : BaseHint
{
    [SerializeField] private Transform hand;
    [SerializeField] private Transform startPos;
    [SerializeField] private List<Transform> targets;
    [SerializeField] private List<VisualAction> targetActions;
    [SerializeField] private float duration = 1.5f;
    
    private int currentPoint = 0;
    private Coroutine moveHand;
    
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.hand.position = this.startPos.position;
        this.moveHand = StartCoroutine(MoveObjectThroughWaypoints());
        await UniTask.WaitUntil(() => Completed, PlayerLoopTiming.Update,cancellationToken);
        StopCoroutine(this.moveHand);
        this.hand.gameObject.SetActive(false);
    }
    
    IEnumerator MoveObjectThroughWaypoints()
    {
        while (true)
        {
            var tempTarget = GetTargetPos();
            this.hand.DOMove(tempTarget.position, this.duration).OnComplete(() =>
            {
                this.hand.gameObject.SetActive(false);
            });
            yield return new WaitForSeconds(this.duration);
            this.hand.position = this.startPos.position;
            this.hand.gameObject.SetActive(true);
        }
    }

    private Transform GetTargetPos()
    { 
        Transform tempPos = this.targets[0].transform;
        for (int i = 0; i < this.targetActions.Count; i++)
        {
            if (this.targetActions[i].Completed)
            {
                this.targetActions.Remove(this.targetActions[i]);
                this.targets.Remove(this.targets[i]);
            }

            tempPos = this.targets[0].transform;
        }
        return tempPos;
    }
}

