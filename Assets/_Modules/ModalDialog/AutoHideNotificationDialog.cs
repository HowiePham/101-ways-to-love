using Cysharp.Threading.Tasks;
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

        [Header("Moving Effect")] [SerializeField]
        private float duration;

        [SerializeField] private float upDistance;

        [Header("Scaling Effect")] [SerializeField]
        private float scalingDuration;

        [SerializeField] private Vector3 targetScale;
        [SerializeField] private bool useScaleEffect;

        public override void Show()
        {
            base.Show();
            float targetY = this.rectTransform.localPosition.y + this.upDistance;
            this.rectTransform.DOLocalMoveY(targetY, this.duration);
            RunScaleEffect();
            Invoke(nameof(Hide), this.duration);
        }

        private async UniTask RunScaleEffect()
        {
            if (this.useScaleEffect)
            {
                this.rectTransform.localScale = Vector3.one;
                await this.rectTransform.DOScale(this.targetScale, this.scalingDuration * 2 / 3).AsyncWaitForCompletion();
                await this.rectTransform.DOScale(Vector3.one, this.scalingDuration * 1 / 3).AsyncWaitForCompletion();
            }
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