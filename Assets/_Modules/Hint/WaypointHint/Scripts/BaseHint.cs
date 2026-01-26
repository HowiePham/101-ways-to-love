using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
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
            EnableHint(true);
        }

        protected void FingerDownHandler(LeanFinger finger)
        {
            EnableHint(false);
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            LeanTouch.OnFingerDown += FingerDownHandler;
            LeanTouch.OnFingerUp += FingerUpHandler;
            await UniTask.CompletedTask;
        }

        protected virtual void StopListeningEvent()
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
        }
    }
}