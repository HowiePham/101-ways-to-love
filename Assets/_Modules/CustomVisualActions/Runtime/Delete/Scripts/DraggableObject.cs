using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Common;
using Lean.Touch;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    [SerializeField] private LeanSelectable selectable;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool backToStartPos;
    
    private Vector2 startPosition;
    private Vector3 velocity = Vector3.zero;

    private void OnEnable()
    {
        this.startPosition = transform.position;
        LeanTouch.OnFingerUpdate += FingerUpdateHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
    }

    private void FingerUpdateHandler(LeanFinger finger)
    {
        if (!this.selectable.IsSelected) return;
        var fingerPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        var targetPos = new Vector3(fingerPos.x + this.offset.x, fingerPos.y + this.offset.y, 0);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref this.velocity, this.smoothTime, Mathf.Infinity);
    }
    
    private void FingerUpHandler(LeanFinger finger)
    {
        if (!this.selectable.IsSelected) return;
        this.selectable.Deselect();
        if (this.backToStartPos)
        {
            ReturnToStartPosition();
        }
    }

    private async void ReturnToStartPosition()
    {
        await UniTask.Delay(100);
        transform.DOMove(this.startPosition, this.smoothTime);
    }
}
