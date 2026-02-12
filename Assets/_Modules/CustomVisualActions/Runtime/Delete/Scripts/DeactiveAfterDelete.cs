using Lean.Touch;
using ScratchCardAsset;
using UnityEngine;

public class DeactiveAfterDelete : MonoBehaviour
{
    [SerializeField] private EraseProgress eraseProgress;
    [SerializeField] private GameObject targetObject;
    [SerializeField, Range(0f, 1f)] public float targetDeletePercentage = 0.8f;
    
    private void Start()
    {
        LeanTouch.OnFingerUp += FingerUpHandler;
    }

    private void FingerUpHandler(LeanFinger obj)
    {
        if (this.targetObject == null) return;
        if (this.eraseProgress.GetProgress() > this.targetDeletePercentage)
        {
            this.targetObject.SetActive(false);
        }
    }
}
