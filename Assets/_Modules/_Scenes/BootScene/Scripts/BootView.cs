using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MEC;
using UnityEngine;
using UnityEngine.UI;

namespace Mimi.Prototypes
{
    public class BootView : MonoBehaviour
    {
        [SerializeField] private RectTransform logo;
        [SerializeField] private RectTransform loadingBar;
        [SerializeField] private Image loadingFillImage;
        [SerializeField] private GameObject[] loadingDotObjects;

        private CoroutineHandle animateDotHandle;

        public void Show()
        {
            this.loadingBar.localScale = Vector3.zero;
            this.loadingFillImage.fillAmount = 0;
            this.logo.localScale = Vector3.zero;
            if (this.loadingDotObjects.Length > 0)
            {
                this.animateDotHandle = Timing.RunCoroutine(_AnimateLoadingDots());
            }
        }

        public async UniTask RunLogoEffect()
        {
            await UniTask.WaitForSeconds(0.2f);
            await this.logo.DOScale(1.2f, 0.4f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
            await this.logo.DOScale(1f, 0.2f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
            this.logo.DOScale(1.05f, 0.4f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }

        public async UniTask ShowLoadingBarEffect()
        {
            await this.loadingBar.DOScale(1.2f, 0.4f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
            await this.loadingBar.DOScale(1f, 0.2f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        }

        public void Hide()
        {
            if (this.animateDotHandle.IsValid)
            {
                Timing.KillCoroutines(this.animateDotHandle);
            }
        }

        public void SetLoadingPercentage(float percentage)
        {
            this.loadingFillImage.fillAmount = percentage;
        }

        private IEnumerator<float> _AnimateLoadingDots()
        {
            int currentDotIndex = 0;

            while (true)
            {
                bool needReset = currentDotIndex == this.loadingDotObjects.Length;

                if (needReset)
                {
                    currentDotIndex = 0;
                    for (int i = 1; i < this.loadingDotObjects.Length; i++)
                    {
                        this.loadingDotObjects[i].SetActive(false);
                    }
                }
                else
                {
                    this.loadingDotObjects[currentDotIndex].SetActive(true);
                    currentDotIndex++;
                }

                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}