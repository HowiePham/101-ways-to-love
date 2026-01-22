using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Hint
{
    public class ShowHintPathOldMechanic : VisualAction
    {
        [SerializeField] private VisualAction hintedAction;
        [SerializeField] private SpriteRenderer renderer;
        [SerializeField, MinValue(0.1f)] private float moveDuration;
        [SerializeField, MinValue(0.1f)] private float fadeDuration;

        public bool HintedActionComplete => this.hintedAction.Completed;

        private Vector3[] path;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            this.renderer.enabled = false;

            if (this.hintedAction is IPathHintable pathHintable)
            {
                this.path = pathHintable.Path;
            }
            else
            {
                this.path = Array.Empty<Vector3>();
            }
        }

        protected override async UniTask OnEnter(CancellationToken cancellationToken)
        {
            await base.OnEnter(cancellationToken);
            LeanTouch.OnFingerDown += FingerDownHandler;
            LeanTouch.OnFingerUp += FingerUpHandler;
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.renderer.enabled = true;
            this.renderer.transform.position = this.path[0];
            this.renderer.color = Color.white;
            var sequence = DOTween.Sequence(gameObject);
            sequence.Append(this.renderer.transform.DOPath(this.path, this.moveDuration))
                .Append(this.renderer.DOFade(0f, this.fadeDuration)).SetLoops(-1, LoopType.Restart);

            try
            {
                await UniTask.WaitUntil(() => this.hintedAction.Completed, PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
            finally
            {
                LeanTouch.OnFingerDown -= FingerDownHandler;
                LeanTouch.OnFingerUp -= FingerUpHandler;
                this.renderer.enabled = false;
                DOTween.Kill(gameObject);
            }
        }

        private void FingerUpHandler(LeanFinger finger)
        {
            this.renderer.enabled = true;
        }

        private void FingerDownHandler(LeanFinger finger)
        {
            this.renderer.enabled = false;
        }
    }
}