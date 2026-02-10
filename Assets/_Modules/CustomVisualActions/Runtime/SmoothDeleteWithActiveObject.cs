using Lean.Touch;
using Mimi.VisualActions.Deleting;
using UnityEngine;

public class SmoothDeleteWithActiveObject : VisualSmoothDelete
{
    [SerializeField] private GameObject eraseObject;

    protected override void FingerDownHandler(LeanFinger finger)
    {
        base.FingerDownHandler(finger);
        if (finger.IsOverGui) return;

        this.eraseObject.SetActive(true);
    }

    protected override void FingerUpHandler(LeanFinger finger)
    {
        base.FingerUpHandler(finger);
        if (finger.IsOverGui) return;

        this.eraseObject.SetActive(false);
    }

    protected override void FingerUpdateHandler(LeanFinger finger)
    {
        base.FingerUpdateHandler(finger);

        if (!this.isFingerDowned) return;
        if (finger.IsOverGui) return;

        this.eraseObject.transform.position = finger.GetWorldPosition(10f, Camera.main);
    }
}