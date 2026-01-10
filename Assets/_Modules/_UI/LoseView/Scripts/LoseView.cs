using System;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Modules._UI.LoseView.Scripts
{
    public class LoseView : BaseView
    {
        [SerializeField] private Button tryAgainButton;

        public Action OnTryAgainClicked;

        public override void Initialize()
        {
            base.Initialize();

            this.tryAgainButton.onClick.AddListener(() => this.OnTryAgainClicked?.Invoke());
        }
    }
}