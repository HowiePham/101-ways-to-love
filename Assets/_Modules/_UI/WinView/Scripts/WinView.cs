using System;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Modules._UI.WinView.Scripts
{
    public class WinView : BaseView
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button continueButton;

        public Action OnContinueClicked;

        public override void Initialize()
        {
            base.Initialize();

            this.continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        }

        public void SetLevelText(int levelOrder)
        {
            this.levelText.text = "Level " + levelOrder;
        }
    }
}