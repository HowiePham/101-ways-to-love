using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;

namespace Mimi.VisualActions.Tapping
{
    public class TapScreen : VisualAction
    {
        private bool complete;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                this.complete = false;
                LeanTouch.OnFingerTap += FingerTapHandler;
                await UniTask.WaitUntil(() => this.complete, PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
            finally
            {
                LeanTouch.OnFingerTap -= FingerTapHandler;
            }
        }

        private void FingerTapHandler(LeanFinger finger)
        {
            this.complete = true;
        }
    }
}