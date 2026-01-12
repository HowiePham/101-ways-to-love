using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.UI;

namespace Mimi.Prototypes
{
    public class BootView : MonoBehaviour
    {
        [SerializeField] private Image loadingFillImage;
        [SerializeField] private GameObject[] loadingDotObjects;

        private CoroutineHandle animateDotHandle;

        public void Show()
        {
            if (this.loadingDotObjects.Length > 0)
            {
                this.animateDotHandle = Timing.RunCoroutine(_AnimateLoadingDots());
            }
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