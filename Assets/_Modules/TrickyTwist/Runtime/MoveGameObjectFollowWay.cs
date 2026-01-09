using System;
using DG.Tweening;
using UnityEngine;

public class MoveGameObjectFollowWay : MonoBehaviour
{
    [SerializeField] private Transform moveObject;
    [SerializeField] private MovingWay movingWay;
    [SerializeField] private float duration = 1f;
    [SerializeField] private PathType pathType = PathType.Linear;
    [SerializeField] private Ease ease = Ease.Linear;

    private Tween movingTween;
    // [SerializeField, ValueDropdown("GetSoundGroups")]
    // private string moveSFX;

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
            .SetEase(this.ease);
    }

    private void OnDisable()
    {
        this.movingTween?.Kill();
    }
}