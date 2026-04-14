using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterUnlockView : BaseView
{
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private Image chapterIcon;
    [SerializeField] private Image chapterIconDark;
    [SerializeField] private Image lockBarIcon;
    [SerializeField] private Image highlightFx;
    [SerializeField] private RectTransform unlockIconPanel;
    [SerializeField] private TMP_Text chapterTitleText;
    [SerializeField] private CanvasGroup continueBtnGroup;
    [SerializeField] private CanvasGroup backHomeBtnGroup;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button backHomeButton;
    [SerializeField] private float lockBarSlideUpOffset = 80f;
    [SerializeField] private float darkIconAlpha = 0.9f;

    [Header("Sound")] [SerializeField, SoundKey]
    private string musicSoundKey;

    [SerializeField, SoundKey] private string unlockSoundKey;
    [SerializeField, SoundKey] private string highlightFxSoundKey;

    private IAudioService AudioService => ServiceLocator.Global.Get<IAudioService>();
    private CancellationTokenSource showCts;
    private TweenerCore<Vector3, Vector3, VectorOptions> continuePulseTween;
    private RectTransform ContinueBtnRect => this.continueBtnGroup.GetComponent<RectTransform>();
    private bool autoMode;
    private UniTaskCompletionSource autoCompletionSource;

    public Action OnContinueClicked;
    public Action OnBackHomeClicked;
    public Action OnShowingCompleted;

    public override void Initialize()
    {
        base.Initialize();

        this.continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        this.backHomeButton.onClick.AddListener(() => OnBackHomeClicked?.Invoke());
    }

    public void SetChapterData(Sprite icon, string title)
    {
        this.chapterIcon.sprite = icon;
        this.chapterIconDark.sprite = icon;
        this.chapterTitleText.SetText(title);
    }

    public UniTask ShowAuto()
    {
        this.autoMode = true;
        this.autoCompletionSource = new UniTaskCompletionSource();
        Show();
        return this.autoCompletionSource.Task;
    }

    public override void Show()
    {
        base.Show();

        this.showCts?.Cancel();
        this.showCts?.Dispose();
        this.showCts = new CancellationTokenSource();

        PlayAudio(this.musicSoundKey);
        HandleUIEffect(this.showCts.Token).Forget();
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
        this.continueBtnGroup.DOFade(0f, 0f);
        this.backHomeBtnGroup.DOFade(0f, 0f);
        
        DOTween.Kill(this.unlockIconPanel);
        this.unlockIconPanel.localRotation = Quaternion.identity;
        DOTween.Kill(this.lockBarIcon.rectTransform);
        DOTween.Kill(this.chapterIcon);
        DOTween.Kill(this.chapterIconDark);
        DOTween.Kill(this.highlightFx);
    }

    private async UniTask HandleUIEffect(CancellationToken ct)
    {
        // === Setup initial locked state ===
        this.contentPanel.localScale = Vector3.zero;
        this.continueBtnGroup.DOFade(0f, 0f);
        this.backHomeBtnGroup.DOFade(0f, 0f);

        // Dark icon visible, bright icon hidden
        this.chapterIconDark.gameObject.SetActive(true);
        this.chapterIconDark.DOFade(this.darkIconAlpha, 0);

        // Lock panel visible, lock bar at original position
        this.unlockIconPanel.gameObject.SetActive(true);
        this.unlockIconPanel.localScale = Vector3.one;
        var lockBarOriginalPos = this.lockBarIcon.rectTransform.anchoredPosition;

        // Highlight hidden
        this.highlightFx.color = new Color(1f, 1f, 1f, 0f);

        // === Phase 1: Scale up content (locked state visible) ===
        await this.contentPanel.DOScale(1f, 0.4f).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        await UniTask.Delay(300, cancellationToken: ct);
        if (ct.IsCancellationRequested) return;

        // === Phase 2: Unlock icon panel shakes before unlock ===
        await this.unlockIconPanel.DOShakeAnchorPos(0.5f, 10f, 15, fadeOut: false)
            .AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        // === Phase 3: Lock bar slides up (simulate unlock action) ===
        PlayAudio(this.unlockSoundKey);
        await this.lockBarIcon.rectTransform
            .DOAnchorPosY(lockBarOriginalPos.y + this.lockBarSlideUpOffset, 0.4f)
            .SetEase(Ease.InBack)
            .AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        // === Phase 4: Unlock icon panel rotates side to side then disappears ===
        await this.unlockIconPanel.DORotate(new Vector3(0f, 0f, 15f), 0.1f, RotateMode.Fast)
            .SetEase(Ease.InOutSine)
            .SetLoops(8, LoopType.Yoyo)
            .AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        this.unlockIconPanel.localRotation = Quaternion.identity;
        await this.unlockIconPanel.DOScale(0f, 0.3f)
            .SetEase(Ease.InBack)
            .AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;
        this.unlockIconPanel.gameObject.SetActive(false);

        // Reset lock bar position for next show
        this.lockBarIcon.rectTransform.anchoredPosition = lockBarOriginalPos;

        // === Phase 4: Dark icon fades out, bright icon + highlight fade in ===
        PlayAudio(this.highlightFxSoundKey);
        this.chapterIconDark.DOFade(0f, 0.3f);
        this.highlightFx.DOFade(1f, 0.4f);
        await UniTask.Delay(400, cancellationToken: ct);
        OnShowingCompleted?.Invoke();
        if (ct.IsCancellationRequested) return;

        if (this.autoMode)
        {
            this.autoMode = false;
            this.autoCompletionSource.TrySetResult();
            return;
        }

        // === Phase 5: Buttons fade in + continue button pulse ===
        this.backHomeBtnGroup.DOFade(1f, 0.5f);
        await this.continueBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        this.ContinueBtnRect.localScale = Vector3.one;
        this.continuePulseTween = this.ContinueBtnRect.DOScale(1.1f, 1f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void PlayAudio(string audioKey)
    {
        if (String.IsNullOrEmpty(audioKey))
        {
            return;
        }

        AudioService.PlaySound(audioKey);
    }
}