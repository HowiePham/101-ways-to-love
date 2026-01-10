using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mimi.Prototypes.UI
{
    public class YesNoDialog : SimpleAnimModalDialog
    {
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private TMP_Text yestText;
        [SerializeField] private TMP_Text noText;
        [SerializeField] private UnityEvent onClickYesEvent;
        [SerializeField] private UnityEvent onClickNoEvent;

        private void Awake()
        {
            this.yesButton.onClick.AddListener(OnClickYes);
            this.noButton.onClick.AddListener(OnClickNo);
        }

        public void SetYesCallback(UnityAction action)
        {
            this.onClickYesEvent.AddListener(action);
        }

        public void SetNoCallback(UnityAction action)
        {
            this.onClickNoEvent.AddListener(action);
        }

        public void SetContentText(string text)
        {
            this.contentText.text = text;
        }

        public void SetYesText(string text)
        {
            this.yestText.text = text;
        }

        public void SetNoText(string text)
        {
            this.noText.text = text;
        }

        private void OnClickYes()
        {
            this.onClickYesEvent.Invoke();
            Hide();
        }

        private void OnClickNo()
        {
            this.onClickNoEvent.Invoke();
            Hide();
        }


        public override void Hide()
        {
            base.Hide();
            this.onClickYesEvent.RemoveAllListeners();
            this.onClickNoEvent.RemoveAllListeners();
        }

        protected override void OnHideComplete()
        {
            ServiceLocator.Global.Get<IPoolService>().Despawn(gameObject);
        }
    }
}