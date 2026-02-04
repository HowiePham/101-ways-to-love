using System;
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
            this.resultView.localScale = Vector3.zero;
            this.continueBtnGroup.DOFade(0f, 0f);
            this.replayBtnGroup.DOFade(0f, 0f);
            this.removeAdsBtnGroup.DOFade(0f, 0f);
            this.settingBtnGroup.DOFade(0f, 0f);

            base.Show();

            await DOTween.Sequence().Append(this.resultView.DOScale(1f, 0.4f)).AsyncWaitForCompletion();

            this.continueBtnGroup.DOFade(1f, 0.5f);
            this.replayBtnGroup.DOFade(1f, 0.5f);
            this.removeAdsBtnGroup.DOFade(1f, 0.5f);
            this.settingBtnGroup.DOFade(1f, 0.5f);
        }
    }
}