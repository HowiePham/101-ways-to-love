using Lean.Touch;
using UnityEngine;
using VisualFlow;

public class SmoothDeleteWithActiveObject : SmoothDelete
{
    [SerializeField] private GameObject eraseObject;
    [SerializeField] private Vector3 offset;

    public override void FingerDownHandler(LeanFinger finger)
    {
        base.FingerDownHandler(finger);
        if (finger.IsOverGui) return;

        this.eraseObject.SetActive(true);
    }

    protected override void OverrideScratchPositionForSelectable()
    {
        if (this.eraseObject == null) return;

        this.scratchCard.Input.OnScratch -= this.scratchCard.ScratchData.GetScratchPosition;
        this.scratchCard.Input.OnScratch += GetSelectableScratchPosition;
    }

    protected override Vector2 GetSelectableScratchPosition(Vector2 fingerScreenPos)
    {
        if (this.eraseObject != null && this.eraseObject.activeSelf)
        {
            Vector2 selectableScreenPos = mainCamera.WorldToScreenPoint(this.eraseObject.transform.position);
            return this.scratchCard.ScratchData.GetScratchPosition(selectableScreenPos);
        }

        return this.scratchCard.ScratchData.GetScratchPosition(fingerScreenPos);
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

        Vector3 fingerPos = finger.GetWorldPosition(10f, Camera.main);
        this.eraseObject.transform.position = new Vector3(fingerPos.x + this.offset.x, fingerPos.y + this.offset.y, fingerPos.z + this.offset.z);
    }
}