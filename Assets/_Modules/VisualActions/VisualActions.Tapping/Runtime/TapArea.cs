using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualActions.Areas;
#if UNITY_EDITOR
#endif

namespace Mimi.VisualActions.Tapping
{
    public class TapArea : VisualAction, IWaypointAction
    {
        [SerializeField, InlineEditor(InlineEditorModes.FullEditor), Required]
        private BaseArea target;

        private bool complete;

        public IEnumerable<Vector3> Waypoints { private set; get; }

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            Waypoints = new[] {this.target.Center};
        }

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
            if (!this.target.ContainsScreenPosition(finger.ScreenPosition, Camera.main)) return;
            this.complete = true;
        }
    }
}