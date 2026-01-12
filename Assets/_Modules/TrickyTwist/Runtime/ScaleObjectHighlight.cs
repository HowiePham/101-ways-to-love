using DG.Tweening;
using UnityEngine;

public class ScaleObjectHighlight : MonoBehaviour
{
    [SerializeField] private Transform highlightTarget;
    [SerializeField] private Vector3 maxScale = new Vector3(1.1f, 1.1f, 1);
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private bool enableHighlight = true;
    private Tween scalingTween;

    public bool EnableHighlight
    {
        get => this.enableHighlight;
        set => this.enableHighlight = value;
    }

    private void OnEnable()
    {
        if (!this.EnableHighlight)
        {
            return;
        }

        this.scalingTween = this.highlightTarget.DOScale(this.maxScale, this.duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(this.ease);
    }

    private void OnDisable()
    {
        this.scalingTween?.Kill();
    }
}