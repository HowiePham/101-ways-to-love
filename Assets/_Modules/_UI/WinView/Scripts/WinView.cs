using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button replayButton;

        public Action OnContinueClicked;
        public Action OnReplayClicked;
        public Action OnRemoveAdsClicked;
        public Action OnSettingClicked;

        public override void Initialize()
        {
            base.Initialize();

            this.continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
            this.replayButton.onClick.AddListener(() => OnReplayClicked?.Invoke());
            this.settingButton.onClick.AddListener(() => this.OnSettingClicked?.Invoke());
            this.removeAdsButton.onClick.AddListener(() => OnRemoveAdsClicked?.Invoke());
        }

        public override async void Show()
        {
            base.Show();

            HandleUIEffect();
        }

        private async UniTask HandleUIEffect()
        {
            this.resultView.localScale = Vector3.zero;
            this.continueBtnGroup.DOFade(0f, 0f);
            this.replayBtnGroup.DOFade(0f, 0f);
            this.removeAdsBtnGroup.DOFade(0f, 0f);
            this.settingBtnGroup.DOFade(0f, 0f);

            await DOTween.Sequence().Append(this.resultView.DOScale(1f, 0.4f)).AsyncWaitForCompletion();

            this.continueBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            this.replayBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            this.removeAdsBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            await this.settingBtnGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();

            LoopScalingUIEffect(this.removeAdsButton.GetComponent<RectTransform>(), 1.1f, 0, 1f);
            LoopScalingUIEffect(this.continueButton.GetComponent<RectTransform>(), 1.1f, 0, 1f);
        }
        
        private async UniTask LoopScalingUIEffect(RectTransform uiItem, float targetValue, float delay, float duration)
        {
            uiItem.localScale = Vector3.one;
            await UniTask.WaitForSeconds(delay);
            uiItem.DOScale(targetValue, duration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }
}