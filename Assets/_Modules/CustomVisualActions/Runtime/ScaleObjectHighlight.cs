using DG.Tweening;
using UnityEngine;

public class ScaleObjectHighlight : MonoBehaviour
{
    [SerializeField] private Transform highlightTarget;
    [SerializeField] private Vector3 maxScale = new Vector3(1.1f, 1.1f, 1);
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private bool enableHighlight = true;

    private Vector3 defaultScale;
    private Tween scalingTween;

    public bool EnableHighlight
    {
        get => this.enableHighlight;
        set
        {
            if (this.enableHighlight == value) return;
            this.enableHighlight = value;
            if (!this.isActiveAndEnabled) return;
            if (value)
            {
                this.StartHighlight();
            }
            else
            {
                this.StopHighlight();
            }
        }
    }

    private void Awake()
    {
        this.defaultScale = this.highlightTarget.localScale;
    }

    private void OnEnable()
    {
        if (this.enableHighlight)
            this.StartHighlight();
    }

    private void OnDisable()
    {
        this.StopHighlight();
    }

    private void StartHighlight()
    {
        this.scalingTween?.Kill();
        this.scalingTween = this.highlightTarget.DOScale(this.maxScale, this.duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(this.ease);
    }

    private void StopHighlight()
    {
        this.scalingTween?.Kill();
        this.scalingTween = null;
        this.highlightTarget.DOScale(this.defaultScale, 0.2f);
    }
}