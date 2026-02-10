using Lean.Touch;
using Mimi.VisualActions;
using Mimi.VisualActions.Tapping;
using UnityEngine;

public class TapAreaWithCondition : TapArea
{
    [SerializeField] private VisualCondition condition;

    protected override void FingerTapHandler(LeanFinger finger)
    {
        if (!this.Target.Active || finger.IsOverGui)
        {
            return;
        }

        if (!this.Target.ContainsScreenPosition(finger.ScreenPosition, Camera.main) || !this.condition.Validate())
        {
            return;
        }

        this.complete = true;
    }
}