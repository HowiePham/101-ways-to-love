using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterUnlockView : BaseView
{
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private Image chapterIcon;
    [SerializeField] private TMP_Text chapterTitleText;
    [SerializeField] private CanvasGroup continueBtnGroup;
    [SerializeField] private CanvasGroup backHomeBtnGroup;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button backHomeButton;

    private CancellationTokenSource showCts;
    private TweenerCore<Vector3, Vector3, VectorOptions> continuePulseTween;
    private RectTransform ContinueBtnRect => this.continueBtnGroup.GetComponent<RectTransform>();

    public Action OnContinueClicked;
    public Action OnBackHomeClicked;

    public override void Initialize()
    {
        base.Initialize();

        this.continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        this.backHomeButton.onClick.AddListener(() => OnBackHomeClicked?.Invoke());
    }

    public void SetChapterData(Sprite icon, string title)
    {
        this.chapterIcon.sprite = icon;
        this.chapterTitleText.SetText(title);
    }

    public override void Show()
    {
        base.Show();

        this.showCts?.Cancel();
        this.showCts?.Dispose();
        this.showCts = new CancellationTokenSource();

        HandleUIEffect(this.showCts.Token);
    }

    public override void Hide()
    {
        base.Hide();

        this.showCts?.Cancel();
        this.showCts?.Dispose();
        this.showCts = null;

        this.continuePulseTween?.Kill();
        this.continuePulseTween = null;

        DOTween.Kill(this.ContinueBtnRect);
        this.ContinueBtnRect.localScale = Vector3.one;
    }

    private async UniTask HandleUIEffect(CancellationToken ct)
    {
        this.contentPanel.localScale = Vector3.zero;
        this.continueBtnGroup.DOFade(0f, 0f);
        this.backHomeBtnGroup.DOFade(0f, 0f);

        await DOTween.Sequence().Append(this.contentPanel.DOScale(1f, 0.4f)).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        this.backHomeBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
        await this.continueBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        this.ContinueBtnRect.localScale = Vector3.one;
        this.continuePulseTween = this.ContinueBtnRect.DOScale(1.1f, 1f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }
}