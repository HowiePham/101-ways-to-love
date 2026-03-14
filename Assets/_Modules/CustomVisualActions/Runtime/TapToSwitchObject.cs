using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Mimi.VisualActions;
using UnityEngine;
using VisualActions.Areas;

namespace _Modules.VisualFlow.Mechanics.FindSortMatch
{
    public class TapToSwitchObject : VisualAction
    {
        [SerializeField] private Transform[] targetPool;
        [SerializeField] private int currentIndex;
        [SerializeField] private BaseArea area;
        [SerializeField] private Transform firstDestination;
        [SerializeField] private Transform targetDestination;
        [SerializeField] private Transform hidingDestination;
        [SerializeField] private float switchDuration;
        [SerializeField] private Ease switchingEase;
        private bool isCompleted;

        public int CurrentIndex => this.currentIndex;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.isCompleted = false;
            Init();
            LeanTouch.OnFingerDown += FingerDownHandler;
            await UniTask.WaitUntil(() => this.isCompleted, cancellationToken: cancellationToken);
        }

        private void Init()
        {
            for (int i = 0; i < this.targetPool.Length; i++)
            {
                Transform target = this.targetPool[i];
                if (this.currentIndex == i)
                {
                    target.position = this.targetDestination.position;
                }
                else
                {
                    target.position = this.firstDestination.position;
                }
            }
        }

        protected override UniTask OnExit(CancellationToken cancellationToken)
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
            return base.OnExit(cancellationToken);
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
        }

        private void FingerDownHandler(LeanFinger finger)
        {
            if (finger.IsOverGui || !this.area.gameObject.activeInHierarchy)
            {
                return;
            }

            if (!this.area.ContainsScreenPosition(finger.ScreenPosition, Camera.main))
            {
                return;
            }

            Transform currentTarget = this.targetPool[this.currentIndex];
            currentTarget.DOMove(this.hidingDestination.position, this.switchDuration).SetEase(this.switchingEase);

            this.currentIndex++;
            if (this.currentIndex >= this.targetPool.Length)
            {
                this.currentIndex = 0;
            }

            Transform newTarget = this.targetPool[this.currentIndex];
            newTarget.position = this.firstDestination.position;
            newTarget.DOMove(this.targetDestination.position, this.switchDuration).SetEase(this.switchingEase);
        }
    }
}