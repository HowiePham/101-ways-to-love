using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;

namespace _Modules._UI.TransitionView.Scripts
{
    public class TransitionView : BaseView
    {
        [SerializeField] private RectTransform bgTransitionRect;
        [SerializeField] private Vector3 initialScale;
        [SerializeField] private float transitionDuration;
        [SerializeField] private TextMeshProUGUI transitionText;
        
        public event Action OnTransitionEnd; 
        public async UniTask ShowTransition(Func<UniTask> task,float delaySeconds = 0f)
        {
            bgTransitionRect.localScale = initialScale;
            transitionText.gameObject.SetActive(true);
            await bgTransitionRect.DOScale(Vector3.zero, transitionDuration / 2).SetUpdate(true).AsyncWaitForCompletion().AsUniTask();
            await task();
            await UniTask.Delay(Mathf.RoundToInt(delaySeconds * 1000f));
            transitionText.gameObject.SetActive(false);
            await bgTransitionRect.DOScale(initialScale, transitionDuration / 2).SetUpdate(true).AsyncWaitForCompletion().AsUniTask();
            OnTransitionEnd?.Invoke();
        }

        public void ShowTransition()
        {
            bgTransitionRect.localScale = initialScale;
            transitionText.gameObject.SetActive(true);
            bgTransitionRect.DOScale(Vector3.zero, transitionDuration / 2).OnComplete(() =>
            {
                transitionText.gameObject.SetActive(false);
                bgTransitionRect.DOScale(initialScale, transitionDuration / 2).OnComplete(() =>
                {
                    OnTransitionEnd?.Invoke();
                });
            });
         
        }
    }
}