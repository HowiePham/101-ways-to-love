using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mimi.Prototypes.UI;
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

        private Dictionary<RectTransform, TweenerCore<Vector3, Vector3, VectorOptions>> loopScalingTweens;
        private CancellationTokenSource showCts;
        private RectTransform RemoveAdsRect => this.removeAdsButton.GetComponent<RectTransform>();
        private RectTransform ContinueBtnRect => this.continueBtnGroup.GetComponent<RectTransform>();

        public Action OnContinueClicked;
        public Action OnReplayClicked;
        public Action OnRemoveAdsClicked;
        public Action OnSettingClicked;
        public Action OnHomeClicked;

        public override void Initialize()
        {
            base.Initialize();

            this.loopScalingTweens = new Dictionary<RectTransform, TweenerCore<Vector3, Vector3, VectorOptions>>();
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

            this.continueBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            this.replayBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            this.removeAdsBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            this.homeBtnBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            await this.settingBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            if (ct.IsCancellationRequested) return;

            LoopScalingUIEffect(this.RemoveAdsRect, 1.1f, 0, 1f, ct);
            LoopScalingUIEffect(this.ContinueBtnRect, 1.1f, 0, 1f, ct);
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
        
        public void SetActiveRemoveAdsButton(bool active)
        {
            this.removeAdsBtnGroup.gameObject.SetActive(active);
        }
    }
}