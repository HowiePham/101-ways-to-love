using DG.Tweening;
using UnityEngine;

namespace Mimi.Prototypes.UI
{
    public class SimpleAnimModalDialog : BaseModalDialog
    {
        [SerializeField] protected CanvasGroup bgCanvasGroup;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private float fadeDuration;
        [SerializeField] private float scaleDuration;

        private bool isHiding;

        public override void Show()
        {
            base.Show();
            this.contentRoot.localScale = Vector3.zero;
            this.bgCanvasGroup.alpha = 0f;
            this.bgCanvasGroup.DOFade(1f, this.fadeDuration);
            this.contentRoot.DOScale(Vector3.one, this.scaleDuration).SetEase(Ease.OutExpo);
            this.isHiding = false;
        }

        public override void Hide()
        {
            if (this.isHiding) return;
            base.Hide();
            this.isHiding = true;
            this.bgCanvasGroup.interactable = false;
            this.bgCanvasGroup.blocksRaycasts = false;
            this.bgCanvasGroup.DOFade(0f, this.fadeDuration);
            this.contentRoot.DOScale(Vector3.zero, this.scaleDuration).OnComplete(OnHideComplete).SetEase(Ease.OutExpo);
        }

        protected virtual void OnHideComplete()
        {
        }
    }
}