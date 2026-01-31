using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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

        public bool Completed => this.hintedAction.Completed;

        protected VisualAction HintedAction => this.hintedAction;
        protected abstract void EnableHint(bool enable);

        protected void FingerUpHandler(LeanFinger finger)
        {
            ActiveHint();
        }

        protected void FingerDownHandler(LeanFinger finger)
        {
            DisableHint();
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            Messenger.AddListener(EventKey.ResetAction, ActiveHint);
            Messenger.AddListener(EventKey.AnimationStart, DisableHint);
            LeanTouch.OnFingerDown += FingerDownHandler;
            LeanTouch.OnFingerUp += FingerUpHandler;
            await UniTask.CompletedTask;
        }

        private void DisableHint()
        {
            EnableHint(false);
        }

        private void ActiveHint()
        {
            EnableHint(true);
        }

        protected virtual void StopListeningEvent()
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
            Messenger.RemoveListener(EventKey.ResetAction, ActiveHint);
            Messenger.RemoveListener(EventKey.AnimationStart, DisableHint);
        }
    }
}