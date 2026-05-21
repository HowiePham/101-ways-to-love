using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mimi.Prototypes.UI;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Event = Spine.Event;

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

        [Header("Chapter Progress")] [SerializeField]
        private ChapterProgressBar chapterProgressBar;

        [Header("Chapter Reward")] [SerializeField]
        private RectTransform chapterRewardPanel;

        [SerializeField] private NumberBasedLifeView numberBasedLifeView;
        [SerializeField] private CanvasGroup rewardDarkBg;
        [SerializeField] private RectTransform rewardMainPanel;
        [SerializeField] private CanvasGroup rewardMainPanelGroup;
        [SerializeField] private Button bonusButton;
        [SerializeField] private Button loseBonusButton;
        [SerializeField] private CanvasGroup loseBonusButtonGroup;
        [SerializeField] protected SkeletonGraphic boxSkeletonGraphic;
        [SerializeField] protected SkeletonGraphic rewardSkeletonGraphic;
        [SerializeField] private int track;

        [Header("Reward Box Jump")] [SerializeField]
        private RectTransform rewardBoxRect;

        [SerializeField] private RectTransform rewardStartPoint;
        [SerializeField] private RectTransform rewardTargetPoint;
        [SerializeField] private float rewardBoxJumpPower = 200f;
        [SerializeField] private float rewardBoxJumpDuration = 0.6f;
        [SerializeField] private float rewardBoxStartScale = 0.5f;
        [SerializeField] private float rewardBoxEndScale = 1f;

        [SerializeField, SpineAnimation(dataField = "boxSkeletonGraphic")]
        protected new string openingAnimation;

        [SerializeField, SpineAnimation(dataField = "boxSkeletonGraphic")]
        protected new string boxIdleAnimation;

        [SerializeField, SpineAnimation(dataField = "boxSkeletonGraphic")]
        protected new string rewardIdleAnimation;

        [SerializeField, SpineAnimation(dataField = "rewardSkeletonGraphic")]
        protected new string defaultRewardAnimation;

        [SerializeField, SpineAnimation(dataField = "rewardSkeletonGraphic")]
        protected new string bonusRewardAnimation;

        [SerializeField] protected string lifeChangedEvent;

        private bool showChapterReward;
        private bool isPlayingReward;
        private int currentLife;
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

            if (this.continueBtnGroup != null)
            {
                DOTween.Kill(this.ContinueBtnRect);
                this.ContinueBtnRect.localScale = Vector3.one;
            }

            if (this.removeAdsButton != null)
            {
                DOTween.Kill(this.RemoveAdsRect);
                this.RemoveAdsRect.localScale = Vector3.one;
            }
            if (this.chapterProgressBar != null)
            {
                DOTween.Kill(this.chapterProgressBar.transform);
                this.chapterProgressBar.transform.localScale = Vector3.zero;
            }

            if (this.chapterRewardPanel != null)
            {
                if (this.rewardDarkBg != null)
                {
                    DOTween.Kill(this.rewardDarkBg);
                    this.rewardDarkBg.alpha = 0f;
                }

                if (this.rewardMainPanel != null)
                {
                    DOTween.Kill(this.rewardMainPanel);
                    this.rewardMainPanel.localScale = Vector3.one;
                }

                if (this.rewardMainPanelGroup != null)
                {
                    DOTween.Kill(this.rewardMainPanelGroup);
                    this.rewardMainPanelGroup.alpha = 1f;
                }

                if (this.bonusButton != null)
                {
                    var bonusRect = (RectTransform)this.bonusButton.transform;
                    DOTween.Kill(bonusRect);
                    bonusRect.localScale = Vector3.one;
                    this.bonusButton.interactable = true;
                }

                if (this.loseBonusButtonGroup != null)
                {
                    DOTween.Kill(this.loseBonusButtonGroup);
                    this.loseBonusButtonGroup.alpha = 1f;
                }

                if (this.loseBonusButton != null)
                {
                    this.loseBonusButton.interactable = true;
                }

                if (this.rewardBoxRect != null)
                {
                    DOTween.Kill(this.rewardBoxRect);
                    this.rewardBoxRect.localScale = new Vector3(this.rewardBoxStartScale, this.rewardBoxStartScale, this.rewardBoxStartScale);
                    this.rewardBoxRect.gameObject.SetActive(false);
                }

                if (this.rewardSkeletonGraphic != null)
                {
                    if (this.rewardSkeletonGraphic.AnimationState != null)
                        this.rewardSkeletonGraphic.AnimationState.Event -= PlayLifeNumberEffect;
                    this.rewardSkeletonGraphic.gameObject.SetActive(false);
                }

                if (this.boxSkeletonGraphic != null)
                {
                    this.boxSkeletonGraphic.gameObject.SetActive(true);
                }

                this.chapterRewardPanel.gameObject.SetActive(false);
            }

            this.showChapterReward = false;
            this.isPlayingReward = false;
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

            await DOTween.Sequence().Append(this.resultView.DOScale(1f, 0.4f)).AsyncWaitForCompletion();
            if (ct.IsCancellationRequested) return;

            await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);
            if (ct.IsCancellationRequested) return;

            if (this.chapterProgressBar != null)
            {
                await this.chapterProgressBar.PlayShowAnimation();
                if (ct.IsCancellationRequested) return;
            }

            if (this.showChapterReward)
            {
                await ShowChapterRewardPanelEffect(ct);
            }
            else
            {
                await this.chapterProgressBar.PlayHideAnimation();
                await ShowNormalButtonsEffect(ct);
            }
        }

        private async UniTask ShowChapterRewardPanelEffect(CancellationToken ct)
        {
            this.loseBonusButtonGroup.alpha = 0f;
            var bonusRect = (RectTransform)this.bonusButton.transform;
            bonusRect.localScale = Vector3.zero;

            this.chapterRewardPanel.gameObject.SetActive(true);
            this.rewardDarkBg.alpha = 0f;
            this.rewardMainPanel.localScale = Vector3.one;
            if (this.rewardMainPanelGroup != null) this.rewardMainPanelGroup.alpha = 0f;
            if (this.rewardBoxRect != null) this.rewardBoxRect.gameObject.SetActive(true);
            if (this.boxSkeletonGraphic != null) this.boxSkeletonGraphic.gameObject.SetActive(true);
            if (this.rewardSkeletonGraphic != null) this.rewardSkeletonGraphic.gameObject.SetActive(false);

            await this.rewardDarkBg.DOFade(1f, 0.3f).AsyncWaitForCompletion();
            this.chapterProgressBar.PlayHideAnimation();
            if (ct.IsCancellationRequested) return;

            await JumpRewardBoxAsync(ct);
            if (ct.IsCancellationRequested) return;

            if (this.rewardMainPanelGroup != null)
            {
                await this.rewardMainPanelGroup.DOFade(1f, 0.4f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
                this.rewardBoxRect.gameObject.SetActive(false);
                if (ct.IsCancellationRequested) return;
            }

            await OpenRewardBox(ct);
            if (ct.IsCancellationRequested) return;

            await UniTask.WaitForSeconds(1f, cancellationToken: ct);
            if (ct.IsCancellationRequested) return;

            await bonusRect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            if (ct.IsCancellationRequested) return;

            LoopScalingUIEffect(bonusRect, 1.1f, 0, 1f, ct);

            await UniTask.WaitForSeconds(2f, cancellationToken: ct);
            if (ct.IsCancellationRequested) return;

            await this.loseBonusButtonGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
        }

        private async UniTask OpenRewardBox(CancellationToken ct)
        {
            TrackEntry currentEntry = this.boxSkeletonGraphic.AnimationState.GetCurrent(this.track);
            currentEntry = this.boxSkeletonGraphic.AnimationState.SetAnimation(this.track, this.openingAnimation, false);
            await UniTask.WaitUntil(() => currentEntry.IsComplete, PlayerLoopTiming.Update, ct);
            this.boxSkeletonGraphic.AnimationState.SetAnimation(this.track, this.rewardIdleAnimation, true);
        }

        private async UniTask JumpRewardBoxAsync(CancellationToken ct)
        {
            if (this.rewardBoxRect == null || this.rewardStartPoint == null || this.rewardTargetPoint == null)
                return;

            DOTween.Kill(this.rewardBoxRect);

            Vector3 startScale = Vector3.one * this.rewardBoxStartScale;
            Vector3 endScale = Vector3.one * this.rewardBoxEndScale;
            Vector3 takeoffSquash = new Vector3(startScale.x * 1.15f, startScale.y * 0.85f, startScale.z);

            this.rewardBoxRect.position = this.rewardStartPoint.position;
            this.rewardBoxRect.localScale = startScale;

            await this.rewardBoxRect
                .DOScale(takeoffSquash, 0.1f)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();
            if (ct.IsCancellationRequested) return;

            var jump = this.rewardBoxRect
                .DOJump(this.rewardTargetPoint.position, this.rewardBoxJumpPower, 1, this.rewardBoxJumpDuration)
                .SetEase(Ease.OutCubic);
            this.rewardBoxRect.DOScale(endScale, this.rewardBoxJumpDuration).SetEase(Ease.OutQuad);
            await jump.AsyncWaitForCompletion();
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

            if (this.rewardMainPanelGroup != null)
            {
                await this.rewardMainPanelGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad).AsyncWaitForCompletion();
                if (ct.IsCancellationRequested) return;
            }

            await this.rewardDarkBg.DOFade(0f, 0.2f).AsyncWaitForCompletion();
            if (ct.IsCancellationRequested) return;

            this.boxSkeletonGraphic.AnimationState.SetAnimation(this.track, this.boxIdleAnimation, true);
            this.chapterRewardPanel.gameObject.SetActive(false);

            await ShowNormalButtonsEffect(ct);
            if (ct.IsCancellationRequested) return;

            if (this.rewardSkeletonGraphic != null)
                this.rewardSkeletonGraphic.gameObject.SetActive(false);
            if (this.boxSkeletonGraphic != null)
                this.boxSkeletonGraphic.gameObject.SetActive(true);
            this.isPlayingReward = false;
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

        public async UniTask PlayDefaultRewardAndCloseAsync(CancellationToken ct)
        {
            await PlayRewardAnimationAsync(this.defaultRewardAnimation, ct);
            if (ct.IsCancellationRequested) return;

            await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);
            HideChapterRewardAndShowButtons().Forget();
        }

        public async UniTask PlayBonusRewardAndCloseAsync(CancellationToken ct)
        {
            await PlayRewardAnimationAsync(this.bonusRewardAnimation, ct);
            if (ct.IsCancellationRequested) return;

            await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);
            HideChapterRewardAndShowButtons().Forget();
        }

        public CancellationToken GetShowCancellationToken()
        {
            return this.showCts?.Token ?? CancellationToken.None;
        }

        public void SetChapterButtonsInteractable(bool value)
        {
            if (this.bonusButton != null) this.bonusButton.interactable = value;
            if (this.loseBonusButton != null) this.loseBonusButton.interactable = value;
        }

        private async UniTask PlayRewardAnimationAsync(string animationName, CancellationToken ct)
        {
            if (this.isPlayingReward) return;
            if (this.rewardSkeletonGraphic == null) return;
            if (string.IsNullOrEmpty(animationName)) return;

            this.isPlayingReward = true;
            SetChapterButtonsInteractable(false);

            try
            {
                await HideChapterButtonsAsync(ct);
                if (ct.IsCancellationRequested) return;

                if (this.boxSkeletonGraphic != null)
                    this.boxSkeletonGraphic.gameObject.SetActive(false);

                var animState = this.rewardSkeletonGraphic.AnimationState;
                this.rewardSkeletonGraphic.gameObject.SetActive(true);

                animState.Event += PlayLifeNumberEffect;
                try
                {
                    TrackEntry entry = animState.SetAnimation(this.track, animationName, false);
                    await UniTask.WaitUntil(() => entry == null || entry.IsComplete,
                        PlayerLoopTiming.Update, ct);
                }
                finally
                {
                    animState.Event -= PlayLifeNumberEffect;
                }
            }
            finally
            {
            }
        }

        private async UniTask HideChapterButtonsAsync(CancellationToken ct)
        {
            var bonusRect = (RectTransform)this.bonusButton.transform;
            if (this.loopScalingTweens.TryGetValue(bonusRect, out var pulse))
            {
                pulse?.Kill();
                this.loopScalingTweens.Remove(bonusRect);
            }

            DOTween.Kill(bonusRect);
            DOTween.Kill(this.loseBonusButtonGroup);

            this.loseBonusButtonGroup.DOFade(0f, 0.2f).SetEase(Ease.OutQuad);
            await bonusRect.DOScale(0f, 0.2f).SetEase(Ease.InBack).AsyncWaitForCompletion();
        }

        private void PlayLifeNumberEffect(TrackEntry trackEntry, Event e)
        {
            bool eventMatch = string.Equals(e.Data.Name, this.lifeChangedEvent, System.StringComparison.Ordinal);
            if (!eventMatch) return;
            if (this.numberBasedLifeView == null) return;

            this.currentLife++;
            this.numberBasedLifeView.SetLifeCount(this.currentLife);
            this.numberBasedLifeView.PlayLifeGainedEffect();
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

        public void SetChapterRewardData(bool show, int currentLife)
        {
            this.showChapterReward = show;
            this.currentLife = currentLife;
            if (this.numberBasedLifeView != null)
            {
                this.numberBasedLifeView.SetLifeCount(currentLife);
            }
        }
    }
}