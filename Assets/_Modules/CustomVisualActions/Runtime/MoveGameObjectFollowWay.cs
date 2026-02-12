using System;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;

public class MoveGameObjectFollowWay : MonoBehaviour
{
    [SerializeField] private Transform moveObject;
    [SerializeField] private MovingWay movingWay;
    [SerializeField] private float duration = 1f;
    [SerializeField] private PathType pathType = PathType.Linear;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private VisualAction actionCondition;
    private Vector3[] path;
    private Tween movingTween;

    private void OnEnable()
    {
        Transform[] points = this.movingWay.Points;
        this.path = new Vector3[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            this.path[i] = point.position;
        }

        this.moveObject.position = this.path[0];

        StartMoving();
    }

    private void Update()
    {
        if (this.actionCondition == null || !this.actionCondition.Completed)
        {
            return;
        }

        this.movingTween?.Kill();
    }

    public void StartMoving()
    {
        this.movingTween = this.moveObject.DOPath(this.path, this.duration, this.pathType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(this.ease)
            .OnWaypointChange(waypointIndex =>
            {
                if (waypointIndex < this.path.Length)
                {
                    Vector3 direction = this.path[waypointIndex] - this.moveObject.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    this.moveObject.DORotate(new Vector3(0, angle, 0), 0);
                }
            });
    }

    public void ResumeMoving()
    {
        if (this.movingTween == null)
        {
            return;
        }

        this.movingTween.Play();
    }

    public void StopMoving()
    {
        if (this.movingTween == null)
        {
            return;
        }

        this.movingTween.Pause();
    }

    private void OnDisable()
    {
        this.movingTween?.Kill();
    }
}