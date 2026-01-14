using System;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Modules._UI.WinView.Scripts
{
    public class WinView : BaseView
    {
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
    }
}