using System;
using DG.Tweening;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Modules._UI.WinView.Scripts
{
    public class WinView : BaseView
    {
        [SerializeField] private RectTransform resultView;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button replayButton;

        public Action OnContinueClicked;
        public Action OnReplayClicked;

        public override void Initialize()
        {
            base.Initialize();

            this.continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
            this.replayButton.onClick.AddListener(() => OnReplayClicked?.Invoke());
        }

        public override async void Show()
        {
            this.resultView.localScale = Vector3.zero;
            this.continueButton.image.DOFade(0f, 0f);
            this.replayButton.image.DOFade(0f, 0f);
            base.Show();

            await DOTween.Sequence().Append(this.resultView.DOScale(1f, 0.4f)).AsyncWaitForCompletion();
            
            this.continueButton.image.DOFade(1f, 0.5f);
            this.replayButton.image.DOFade(1f, 0.5f);
        }
    }
}