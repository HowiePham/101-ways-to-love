using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mimi.Prototypes.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Modules._UI.WinView.Scripts
{
    public class WinView : BaseView
    {
        [SerializeField] private RectTransform resultView;
        [SerializeField] private CanvasGroup continueBtnGroup;
        [SerializeField] private CanvasGroup replayBtnGroup;
        [SerializeField] private CanvasGroup settingBtnGroup;
        [SerializeField] private CanvasGroup removeAdsBtnGroup;
        [SerializeField] private CanvasGroup homeBtnBtnGroup;
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private RectTransform nextChapterParent;
        [SerializeField] private Image nextChapterImage;

        [Header("Chapter Progress")] [SerializeField]
        private ChapterProgressBar chapterProgressBar;

        [Header("Chapter Reward")] [SerializeField]
        private RectTransform chapterRewardPanel;

        [SerializeField] private Button bonusButton;
        [SerializeField] private Button loseBonusButton;
        [SerializeField] private CanvasGroup loseBonusButtonGroup;
        [SerializeField] private TMP_Text rewardAmountText;

        private bool showChapterReward;
        private int chapterRewardAmount;
        private Dictionary<RectTransform, TweenerCore<Vector3, Vector3, VectorOptions>> loopScalingTweens;
        private CancellationTokenSource showCts;
        private RectTransform RemoveAdsRect => this.removeAdsButton.GetComponent<RectTransform>();
        private RectTransform ContinueBtnRect => this.continueBtnGroup.GetComponent<RectTransform>();

        public Action OnContinueClicked;
        public Action OnReplayClicked;
        public Action OnRemoveAdsClicked;
        public Action OnSettingClicked;
        public Action OnHomeClicked;
        public Action OnBonusClicked;
        public Action OnLoseBonusClicked;

        public override void Initialize()
        {
            base.Initialize();

            this.loopScalingTweens = new Dictionary<RectTransform, TweenerCore<Vector3, Vector3, VectorOptions>>();

            if (this.chapterProgressBar != null)
                this.chapterProgressBar.Initialize();

            if (this.chapterRewardPanel != null)
            {
                this.chapterRewardPanel.gameObject.SetActive(false);
                this.bonusButton.onClick.AddListener(() => OnBonusClicked?.Invoke());
                this.loseBonusButton.onClick.AddListener(() => this.OnLoseBonusClicked?.Invoke());
            }

            this.continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
            this.replayButton.onClick.AddListener(() => OnReplayClicked?.Invoke());
            this.settingButton.onClick.AddListener(() => this.OnSettingClicked?.Invoke());
            this.removeAdsButton.onClick.AddListener(() => OnRemoveAdsClicked?.Invoke());
            this.homeButton.onClick.AddListener(() => OnHomeClicked?.Invoke());
        }

        public override void Show()
        {
            base.Show();

            this.showCts?.Cancel();
            this.showCts?.Dispose();
            this.showCts = new CancellationTokenSource();

            HandleUIEffect(this.showCts.Token);
        }

        [Button]
        private void TestEffect()
        {
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

            foreach (var kvp in this.loopScalingTweens)
            {
                kvp.Value?.Kill();
            }

            this.loopScalingTweens.Clear();

            DOTween.Kill(this.ContinueBtnRect);
            DOTween.Kill(this.RemoveAdsRect);
            this.ContinueBtnRect.localScale = Vector3.one;
            this.RemoveAdsRect.localScale = Vector3.one;
            if (this.chapterProgressBar != null)
            {
                DOTween.Kill(this.chapterProgressBar.transform);
                this.chapterProgressBar.transform.localScale = Vector3.zero;
            }

            if (this.chapterRewardPanel != null)
            {
                DOTween.Kill(this.chapterRewardPanel);
                this.chapterRewardPanel.gameObject.SetActive(false);
                this.chapterRewardPanel.localScale = Vector3.one;
            }

            this.showChapterReward = false;
            this.nextChapterParent.localScale = Vector3.zero;
            this.nextChapterParent.gameObject.SetActive(false);
        }

        private async UniTask HandleUIEffect(CancellationToken ct)
        {
            this.resultView.localScale = Vector3.zero;
            this.continueBtnGroup.DOFade(0f, 0f);
            this.replayBtnGroup.DOFade(0f, 0f);
            this.removeAdsBtnGroup.DOFade(0f, 0f);
            this.settingBtnGroup.DOFade(0f, 0f);
            this.settingBtnGroup.DOFade(0f, 0f);
            this.homeBtnBtnGroup.DOFade(0f, 0f);

            if (this.nextChapterParent.gameObject.activeSelf)
            {
                this.nextChapterParent.localScale = Vector3.zero;
            }

            await DOTween.Sequence().Append(this.resultView.DOScale(1f, 0.4f)).AsyncWaitForCompletion();
            if (ct.IsCancellationRequested) return;

            if (this.nextChapterParent.gameObject.activeSelf)
            {
                await UniTask.WaitForSeconds(0.2f, cancellationToken: ct);

                await DOTween.Sequence()
                    .Append(this.nextChapterParent.DOScale(1.3f, 0.3f).SetEase(Ease.OutQuad))
                    .Append(this.nextChapterParent.DOScale(1.4f, 0.3f).SetEase(Ease.OutQuad))
                    .Append(this.nextChapterParent.DOScale(1f, 0.15f).SetEase(Ease.InOutQuad))
                    .AsyncWaitForCompletion();
                if (ct.IsCancellationRequested) return;

                await UniTask.WaitForSeconds(0.75f, cancellationToken: ct);
            }

            if (this.chapterProgressBar != null)
            {
                await this.chapterProgressBar.PlayShowAnimation();
                if (ct.IsCancellationRequested) return;
            }

            if (this.showChapterReward)
            {
                await ShowChapterRewardPanelEffect(ct);
                // Normal buttons are shown by HideChapterRewardAndShowButtons() after user clicks bonus/lose
            }
            else
            {
                await ShowNormalButtonsEffect(ct);
            }
        }

        private async UniTask ShowChapterRewardPanelEffect(CancellationToken ct)
        {
            if (this.rewardAmountText != null)
                this.rewardAmountText.text = $"+{this.chapterRewardAmount}";

            this.loseBonusButtonGroup.alpha = 0f;

            this.chapterRewardPanel.gameObject.SetActive(true);
            this.chapterRewardPanel.localScale = Vector3.zero;

            await this.chapterRewardPanel.DOScale(1f, 0.4f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            if (ct.IsCancellationRequested) return;

            var bonusRect = (RectTransform)this.bonusButton.transform;
            LoopScalingUIEffect(bonusRect, 1.1f, 0, 1f, ct);

            await UniTask.WaitForSeconds(1f, cancellationToken: ct);
            if (ct.IsCancellationRequested) return;

            await this.loseBonusButtonGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
        }

        public async UniTaskVoid HideChapterRewardAndShowButtons()
        {
            var ct = this.showCts?.Token ?? CancellationToken.None;

            var bonusRect = (RectTransform)this.bonusButton.transform;
            if (this.loopScalingTweens.TryGetValue(bonusRect, out var pulseTween))
            {
                pulseTween?.Kill();
                this.loopScalingTweens.Remove(bonusRect);
            }

            await this.chapterRewardPanel.DOScale(0f, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
            if (ct.IsCancellationRequested) return;

            this.chapterRewardPanel.gameObject.SetActive(false);

            await ShowNormalButtonsEffect(ct);
        }

        private async UniTask ShowNormalButtonsEffect(CancellationToken ct)
        {
            this.removeAdsBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            this.settingBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            await this.continueBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            if (ct.IsCancellationRequested) return;

            LoopScalingUIEffect(this.RemoveAdsRect, 1.1f, 0, 1f, ct);
            LoopScalingUIEffect(this.ContinueBtnRect, 1.1f, 0, 1f, ct);
            await UniTask.WaitForSeconds(1f, cancellationToken: ct);
            if (ct.IsCancellationRequested) return;

            this.replayBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            await this.homeBtnBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
        }

        private async UniTask LoopScalingUIEffect(RectTransform uiItem, float targetValue, float delay, float duration, CancellationToken ct = default)
        {
            uiItem.localScale = Vector3.one;
            bool canceled = await UniTask.WaitForSeconds(delay, cancellationToken: ct).SuppressCancellationThrow();
            if (canceled) return;

            if (this.loopScalingTweens.ContainsKey(uiItem))
            {
                this.loopScalingTweens[uiItem] = uiItem.DOScale(targetValue, duration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                TweenerCore<Vector3, Vector3, VectorOptions> tweenCore = uiItem.DOScale(targetValue, duration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
                this.loopScalingTweens.Add(uiItem, tweenCore);
            }
        }

        public void SetNextChapterHint(bool show, Sprite chapterSprite = null)
        {
            this.nextChapterParent.gameObject.SetActive(show);
            if (show && chapterSprite != null)
            {
                this.nextChapterImage.sprite = chapterSprite;
            }
        }

        public void SetActiveRemoveAdsButton(bool active)
        {
            this.removeAdsBtnGroup.gameObject.SetActive(active);
        }

        public void SetChapterProgress(int completedCount, int totalLevels)
        {
            if (this.chapterProgressBar != null)
                this.chapterProgressBar.SetProgressData(completedCount, totalLevels);
        }

        public void SetChapterRewardData(bool show, int amount)
        {
            this.showChapterReward = show;
            this.chapterRewardAmount = amount;
        }
    }
}