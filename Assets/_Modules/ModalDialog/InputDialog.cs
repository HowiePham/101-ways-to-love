using System;
using TMPro;
using UnityEngine.UI;

namespace Mimi.Prototypes.UI
{
    public class InputDialog :  SimpleAnimModalDialog
    {
        public TextMeshProUGUI contentText;
        public TMP_InputField input;
        public Button          okButton;
        public Button closeButton;

        public Action<string> onOk;
        
        private void Awake()
        {
            closeButton.onClick.AddListener(Hide);
            this.okButton.onClick.AddListener(() =>
            {
                this.onOk?.Invoke(input.text);
                Hide();
            });
        }
        
    }
}