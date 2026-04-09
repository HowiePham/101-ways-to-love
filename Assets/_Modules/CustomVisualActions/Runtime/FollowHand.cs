using System;
using Lean.Touch;
using Mimi.VisualActions;
using UnityEngine;

public class FollowHand : MonoBehaviour
{
    [SerializeField] private VisualAction actionCondition;
    [SerializeField] private Vector3 inputOffset;
    private bool isDragging;

    private void Start()
    {
        LeanTouch.OnFingerDown += StartDragging;
        LeanTouch.OnFingerUp += StopDragging;
        LeanTouch.OnFingerUpdate += MoveFollowHand;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= StartDragging;
        LeanTouch.OnFingerUp -= StopDragging;
        LeanTouch.OnFingerUpdate -= MoveFollowHand;
    }

    private void MoveFollowHand(LeanFinger finger)
    {
        if (!this.isDragging || this.actionCondition.Completed)
        {
            return;
        }

        this.transform.position = finger.GetWorldPosition(10f, Camera.main) + this.inputOffset;
    }

    private void StopDragging(LeanFinger finger)
    {
        this.isDragging = false;
    }

    private void StartDragging(LeanFinger finger)
    {
        if (finger.IsOverGui)
        {
            this.isDragging = false;
            return;
        }

        this.isDragging = true;
    }
}