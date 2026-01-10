using System;
using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mimi.Prototypes.UI
{
    public class NotificationOkDialog : SimpleAnimModalDialog
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private Button okButton;
        [SerializeField] private TMP_Text buttonText;

        private event Action OnHide;

        private void Awake()
        {
            if (this.okButton != null) this.okButton.onClick.AddListener(Hide);
        }

        public override void Hide()
        {
            base.Hide();
            OnHide?.Invoke();
        }

        public void AddOnHideAction(Action action)
        {
            OnHide += action;
        }

        public void SetTitleText(string text)
        {
            this.titleText.text = text;
        }

        public void SetContentText(string text)
        {
            this.contentText.text = text;
        }

        public void SetButtonText(string text)
        {
            this.buttonText.text = text;
        }

        protected override void OnHideComplete()
        {
            ServiceLocator.Global.Get<IPoolService>().Despawn(gameObject);
        }
    }
}