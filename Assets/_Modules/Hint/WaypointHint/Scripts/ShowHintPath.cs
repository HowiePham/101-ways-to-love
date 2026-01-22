using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VisualFlow
{
    public class ShowHintPath : BaseHint
    {
        [SerializeField, Required] private BaseHintGraphic hintGraphic;
        [SerializeField, MinValue(0.1f)] private float moveDuration;

        private Vector3[] path;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            this.hintGraphic.SetActive(false);

            if (HintedAction is IPathHint hint)
            {
                this.path = hint.Path;
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
            this.hintGraphic.transform.position = this.path[0];
            this.hintGraphic.SetActive(true);
            DG.Tweening.Sequence sequence = DOTween.Sequence(gameObject);
            sequence.Append(this.hintGraphic.transform.DOPath(this.path, this.moveDuration))
                .SetLoops(-1, LoopType.Restart);

            try
            {
                await UniTask.WaitUntil(() => 
                    Completed, PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
            finally
            {
                LeanTouch.OnFingerDown -= FingerDownHandler;
                LeanTouch.OnFingerUp -= FingerUpHandler;
                this.hintGraphic.enabled = false;
                this.hintGraphic.SetActive(false);
                DOTween.Kill(gameObject);
            }
        }

        private void FingerUpHandler(LeanFinger finger)
        {
            this.hintGraphic.enabled = true;
        }

        private void FingerDownHandler(LeanFinger finger)
        {
            this.hintGraphic.enabled = false;
        }
    }
}