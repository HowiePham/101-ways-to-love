using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using UnityEngine;
using VisualFlow;

public class SpriteSwapHint : BaseHint
{
    [SerializeField] private Transform hand;
    [SerializeField] private Transform pathParent;
    [SerializeField] private Vector3[] waypoints;
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private SpriteRenderer handSprite;
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite swappedSprite;


    private int currentPoint = 0;
    private Coroutine moveHand;
    private bool isFingerDown;

    protected override async UniTask OnInitializing()
    {
        await base.OnInitializing();
        this.waypoints = CreateVectorHintPath();
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.isFingerDown = false;
        this.hand.position = this.waypoints[0];
        this.moveHand = StartCoroutine(MoveObjectThroughWaypoints());
        LeanTouch.OnFingerDown += FingerDownHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
        await UniTask.WaitUntil(() => Completed, PlayerLoopTiming.Update, cancellationToken);
        LeanTouch.OnFingerDown -= FingerDownHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
        StopCoroutine(this.moveHand);
        this.hand.gameObject.SetActive(false);
    }

    private void FingerDownHandler(LeanFinger obj)
    {
        this.hand.gameObject.SetActive(false);
        isFingerDown = true;
    }

    private void FingerUpHandler(LeanFinger obj)
    {
        isFingerDown = false;
        this.hand.gameObject.SetActive(true);
    }

    IEnumerator MoveObjectThroughWaypoints()
    {
        while (true)
        {
            
            this.handSprite.sprite = originalSprite;
            hand.position = this.waypoints[0];
            if (isFingerDown)
            {
                this.hand.gameObject.SetActive(false);
            }
            else
            {
                this.hand.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(0.2f);
            this.handSprite.sprite = swappedSprite;
            yield return new WaitForSeconds(0.2f);
            this.hand.DOMove(this.waypoints[1], this.duration).OnComplete(() =>
            {
                this.hand.gameObject.SetActive(false);
            });
            yield return new WaitForSeconds(this.duration);
            // this.hand.position = this.waypoints[0];
            // this.hand.gameObject.SetActive(true);
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