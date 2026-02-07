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

    private Tween movingTween;

    private void OnEnable()
    {
        Transform[] points = this.movingWay.Points;
        var path = new Vector3[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            path[i] = point.position;
        }

        this.moveObject.position = path[0];

        this.movingTween = this.moveObject.DOPath(path, this.duration, this.pathType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(this.ease)
            .OnWaypointChange(waypointIndex =>
            {
                if (waypointIndex < path.Length)
                {
                    Vector3 direction = path[waypointIndex] - this.moveObject.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    this.moveObject.DORotate(new Vector3(0, angle, 0), 0);
                }
            });
    }

    private void Update()
    {
        if (this.actionCondition == null || !this.actionCondition.Completed)
        {
            return;
        }

        this.movingTween?.Kill();
    }

    private void OnDisable()
    {
        this.movingTween?.Kill();
    }
}