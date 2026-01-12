using DG.Tweening;
using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using TMPro;
using UnityEngine;

namespace Mimi.Prototypes.UI
{
    public class AutoHideNotificationDialog : BaseModalDialog
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshProUGUI textUi;
        [SerializeField] private float duration;
        [SerializeField] private float upDistance;

        public override void Show()
        {
            base.Show();
            float targetY = this.rectTransform.localPosition.y + this.upDistance;
            this.rectTransform.DOLocalMoveY(targetY, this.duration);
            Invoke(nameof(Hide), this.duration);
        }

        public void SetText(string text)
        {
            this.textUi.text = text;
        }

        public override void Hide()
        {
            base.Hide();
            ServiceLocator.Global.Get<IPoolService>().Despawn(gameObject);
        }
    }
}