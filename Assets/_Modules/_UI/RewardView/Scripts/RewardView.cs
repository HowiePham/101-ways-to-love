using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using TMPro;
using UnityEngine;

public class RewardView : BaseView
{
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private RectTransform heartIcon;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private RectTransform[] bonusHearts;

    [Header("Sound")] [SerializeField, SoundKey]
    private string rewardSoundKey;

    [SerializeField, SoundKey] private string heartPopSoundKey;

    private IAudioService AudioService => ServiceLocator.Global.Get<IAudioService>();
    private CancellationTokenSource showCts;
    private int rewardAmount;

    public Action OnAnimationCompleted;

    public void SetData(int amount)
    {
        this.rewardAmount = amount;
        this.rewardText.SetText(amount.ToString());

        for (int i = 0; i < this.bonusHearts.Length; i++)
        {
            this.bonusHearts[i].gameObject.SetActive(i < amount);
        }
    }

    public override void Show()
    {
        base.Show();

        this.showCts?.Cancel();
        this.showCts?.Dispose();
        this.showCts = new CancellationTokenSource();

        HandleUIEffect(this.showCts.Token).Forget();
    }

    public override void Hide()
    {
        base.Hide();

        this.showCts?.Cancel();
        this.showCts?.Dispose();
        this.showCts = null;

        if (this.contentPanel != null)
        {
            DOTween.Kill(this.contentPanel);
            this.contentPanel.localScale = Vector3.one;
        }

        DOTween.Kill(this.heartIcon);
        DOTween.Kill(this.rewardText);
        DOTween.Kill(this.subtitleText);

        if (this.bonusHearts != null)
        {
            foreach (var heart in this.bonusHearts)
            {
                DOTween.Kill(heart);
            }
        }
    }

    private async UniTask HandleUIEffect(CancellationToken ct)
    {
        // === Reset initial state ===
        this.heartIcon.localScale = Vector3.zero;
        this.rewardText.alpha = 0f;
        this.subtitleText.alpha = 0f;

        if (this.bonusHearts != null)
        {
            for (int i = 0; i < this.rewardAmount && i < this.bonusHearts.Length; i++)
            {
                this.bonusHearts[i].localScale = Vector3.zero;
            }
        }

        // === Phase 1: Fade in background (0.3s) ===
        if (ct.IsCancellationRequested) return;

        // === Phase 2: Heart icon punches in (0.4s) + confetti + sound ===
        PlayAudio(this.rewardSoundKey);

        this.rewardText.DOFade(1f, 0f);
        this.rewardText.rectTransform.DOScale(1f, 0f);
        await this.heartIcon.DOScale(1.3f, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;
        await this.heartIcon.DOScale(1f, 0.15f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        // === Phase 3: Reward text punches in (0.25s) ===
        // this.rewardText.DOFade(1f, 0.2f);
        // await this.rewardText.rectTransform.DOScale(1.15f, 0.15f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
        // if (ct.IsCancellationRequested) return;
        // await this.rewardText.rectTransform.DOScale(1f, 0.1f).AsyncWaitForCompletion();
        // if (ct.IsCancellationRequested) return;

        // === Phase 4: Subtitle fades in ===
        await this.subtitleText.DOFade(1f, 0.25f).AsyncWaitForCompletion();
        if (ct.IsCancellationRequested) return;

        // === Phase 5: Bonus hearts pop in one by one ===
        if (this.bonusHearts != null)
        {
            for (int i = 0; i < this.rewardAmount && i < this.bonusHearts.Length; i++)
            {
                if (ct.IsCancellationRequested) return;
                PlayAudio(this.heartPopSoundKey);
                await this.bonusHearts[i].DOScale(1.2f, 0.12f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                if (ct.IsCancellationRequested) return;
                await this.bonusHearts[i].DOScale(1f, 0.08f).AsyncWaitForCompletion();
                if (ct.IsCancellationRequested) return;
                await UniTask.Delay(150, cancellationToken: ct);
            }
        }

        // === Phase 6: Hold ===
        await UniTask.Delay(1500, cancellationToken: ct);
        if (ct.IsCancellationRequested) return;

        // === Phase 7: Dismiss — all UI scales down together ===
        if (this.contentPanel != null)
        {
            // Whole panel shrinks as one unit — cleanest dismiss
            await this.contentPanel.DOScale(0f, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
        }
        else
        {
            // Fallback: animate each element individually if no contentPanel assigned
            var hideSeq = DOTween.Sequence();
            hideSeq.Join(this.heartIcon.DOScale(0f, 0.3f).SetEase(Ease.InBack));
            hideSeq.Join(this.rewardText.rectTransform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
            hideSeq.Join(this.subtitleText.rectTransform.DOScale(0f, 0.2f).SetEase(Ease.InBack));
            if (this.bonusHearts != null)
            {
                foreach (var heart in this.bonusHearts)
                    hideSeq.Join(heart.DOScale(0f, 0.22f).SetEase(Ease.InBack));
            }

            await hideSeq.AsyncWaitForCompletion();
        }

        if (ct.IsCancellationRequested) return;

        OnAnimationCompleted?.Invoke();
    }

    private void PlayAudio(string audioKey)
    {
        if (string.IsNullOrEmpty(audioKey)) return;
        AudioService.PlaySound(audioKey);
    }
}