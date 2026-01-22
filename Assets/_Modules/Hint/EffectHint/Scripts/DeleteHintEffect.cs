using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VisualFlow;

public class DeleteHintEffect : BaseHint
{
    [SerializeField] private Transform hand;
    [SerializeField] private Transform pathParent;
    [SerializeField] private Vector3[] waypoints;
    [SerializeField] private float duration = 1f;
    
    private int currentPoint = 0;
    private Coroutine moveHand;
    
    protected override async UniTask OnInitializing()
    {
        await base.OnInitializing();
        this.waypoints = CreateVectorHintPath();
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.hand.position = this.waypoints[1];
        this.moveHand = StartCoroutine(MoveObjectThroughWaypoints());
        await UniTask.WaitUntil(() => Completed, PlayerLoopTiming.Update,cancellationToken);
        StopCoroutine(this.moveHand);
        this.hand.gameObject.SetActive(false);
    }
    
    IEnumerator MoveObjectThroughWaypoints()
    {
        while (true)
        {
            for (int i = 0; i < this.waypoints.Length; i++)
            {
                this.hand.DOMove(this.waypoints[i], this.duration).SetEase(Ease.Linear);
                yield return new WaitForSeconds(this.duration);
            }
        }
    }
    
    private Vector3[] CreateVectorHintPath()
    {
        var pathList = new List<Vector3>(this.pathParent.childCount);
        for (int i = 0; i < this.pathParent.childCount; i++)
        {
            Transform pointTrans = this.pathParent.GetChild(i);
            pathList.Add(pointTrans.position);
        }

        return pathList.ToArray();
    }
}
