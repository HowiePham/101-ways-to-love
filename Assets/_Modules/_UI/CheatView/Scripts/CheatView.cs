using System;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Modules._UI.CheatView.Scripts
{
    public class CheatView : BaseView
    {
        [SerializeField] private Button openCheatButton;
        [SerializeField] private Button selectLevelButton;
        [SerializeField] private GameObject cheatPanel;
        [SerializeField] private TMP_InputField levelInputField;

        public Action<string> SelectLevelClicked;

        public override void Initialize()
        {
            base.Initialize();

            this.openCheatButton.onClick.AddListener(SetActiveCheatPanel);
            this.selectLevelButton.onClick.AddListener(SelectLevelCheatHandler);
        }

        private void SelectLevelCheatHandler()
        {
            this.SelectLevelClicked.Invoke(this.levelInputField.text);
        }

        private void SetActiveCheatPanel()
        {
            bool currentActiveState = this.cheatPanel.activeSelf;
            this.cheatPanel.SetActive(!currentActiveState);
        }
    }
}