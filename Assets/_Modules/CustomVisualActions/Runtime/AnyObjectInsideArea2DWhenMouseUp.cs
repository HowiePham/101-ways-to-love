using System;
using System.Collections.Generic;
using Lean.Touch;
using Mimi.VisualActions;
using UnityEngine;
using VisualActions.Areas;

public class AnyObjectInsideArea2DWhenMouseUp : VisualCondition
{
    [SerializeField] private List<Transform> targetObjects;
    [SerializeField] private BaseArea targetArea;
    [SerializeField] private bool isCompleted;

    private void OnEnable()
    {
        LeanTouch.OnFingerUp += FingerUpHandler;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerUp -= FingerUpHandler;
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        if (!this.targetArea.Active)
        {
            this.isCompleted = false;
            return;
        }

        foreach (Transform target in this.targetObjects)
        {
            if (this.targetArea.ContainsWorldSpace(target.position))
            {
                this.isCompleted = true;
                return;
            }
        }

        this.isCompleted = false;
    }

    public override bool Validate()
    {
        return this.isCompleted;
    }
}