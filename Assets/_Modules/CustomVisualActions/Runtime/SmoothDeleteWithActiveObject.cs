using Lean.Touch;
using UnityEngine;
using VisualFlow;

public class SmoothDeleteWithActiveObject : SmoothDelete
{
    [SerializeField] private GameObject eraseObject;

    public override void FingerDownHandler(LeanFinger finger)
    {
        base.FingerDownHandler(finger);
        if (finger.IsOverGui) return;

        this.eraseObject.SetActive(true);
    }

    public override void FingerUpHandler(LeanFinger finger)
    {
        base.FingerUpHandler(finger);
        if (finger.IsOverGui) return;

        this.eraseObject.SetActive(false);
    }

    public override void FingerUpdateHandler(LeanFinger finger)
    {
        base.FingerUpdateHandler(finger);

        if (!this.isFingerDowned) return;
        if (finger.IsOverGui) return;

        this.eraseObject.transform.position = finger.GetWorldPosition(10f, Camera.main);
    }
}