using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Lean.Common;
using Lean.Touch;
using Mimi.Prototypes.Events;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VisualFlow
{
    [DefaultExecutionOrder(1)]
    public abstract class BaseHint : VisualAction
    {
        [SerializeField, Required] protected VisualAction hintedAction;

        protected LeanSelectByFinger leanSelectByFinger;
        public bool Completed => this.hintedAction.Completed;

        protected VisualAction HintedAction => this.hintedAction;
        protected abstract void EnableHint(bool enable);

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();

            if (this.leanSelectByFinger == null)
            {
                this.leanSelectByFinger = FindAnyObjectByType<LeanSelectByFinger>();
            }
        }

        protected virtual void FingerUpHandler(LeanFinger finger)
        {
            ActiveHint();
        }

        protected virtual void FingerDownHandler(LeanFinger finger)
        {
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            Messenger.AddListener(EventKey.ResetAction, ActiveHint);
            Messenger.AddListener(EventKey.LevelWin, TurnOff);
            Messenger.AddListener(EventKey.AnimationStart, DisableHint);
            LeanTouch.OnFingerDown += FingerDownHandler;
            if (this.leanSelectByFinger != null)
            {
                this.leanSelectByFinger.OnSelected.AddListener(OnSelectedHandler);
            }

            LeanTouch.OnFingerUp += FingerUpHandler;
            await UniTask.CompletedTask;
        }

        private void OnSelectedHandler(LeanSelectable leanSelectable)
        {
            DisableHint();
        }

        protected void DisableHint()
        {
            EnableHint(false);
        }

        protected void ActiveHint()
        {
            EnableHint(true);
        }

        private void TurnOff()
        {
            DisableHint();
            StopListeningEvent();
        }

        protected virtual void StopListeningEvent()
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
            Messenger.RemoveListener(EventKey.ResetAction, ActiveHint);
            Messenger.RemoveListener(EventKey.AnimationStart, DisableHint);
            Messenger.RemoveListener(EventKey.LevelWin, TurnOff);
            if (this.leanSelectByFinger != null)
            {
                this.leanSelectByFinger.OnSelected.RemoveListener(OnSelectedHandler);
            }
        }

        protected virtual void OnDestroy()
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
            Messenger.RemoveListener(EventKey.ResetAction, ActiveHint);
            Messenger.RemoveListener(EventKey.AnimationStart, DisableHint);
            Messenger.RemoveListener(EventKey.LevelWin, TurnOff);
            if (this.leanSelectByFinger != null)
            {
                this.leanSelectByFinger.OnSelected.RemoveListener(OnSelectedHandler);
            }
        }
    }
}