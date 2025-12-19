using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    [TypeInfoBox("Handles interaction by holding/moving in a specified area until completion.")]
    public class InteractInArea : VisualAction
    {
        [SerializeField] private GetTimerWhenInteractInsideArea _getTimeInsideArea;
        [SerializeField] private BaseUpdateProgress _updateProgress;
        [SerializeField] private float limitTime;
        private bool isCompleted => _getTimeInsideArea.insideTime >= limitTime;

        /// <summary>
        /// Initializes components for interaction.
        /// </summary>
        /// <returns>UniTask representing async completion.</returns>
        protected override UniTask OnInitializing()
        {
            _getTimeInsideArea.enabled = false;
            this._updateProgress.OnInit();

            return base.OnInitializing();
        }

        /// <summary>
        /// Executes interaction process until time limit reached.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel async operation.</param>
        /// <returns>UniTask representing async completion.</returns>
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this._updateProgress.SetTimeProgress(limitTime);

            _getTimeInsideArea.enabled = true;
            _getTimeInsideArea.OnUpdateProgress += OnUpdateProgress;
            await UniTask.WaitUntil(() => isCompleted, cancellationToken: cancellationToken);
            _getTimeInsideArea.OnUpdateProgress -= OnUpdateProgress;
            _getTimeInsideArea.enabled = false;

            this._updateProgress.CompleteProgress();
        }

        /// <summary>
        /// Updates progress based on interaction.
        /// </summary>
        /// <param name="progress">The current progress value.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <param name="isFingerDown">Whether the finger is down.</param>
        /// <param name="isIncreaseTime">Whether time is increasing.</param>
        public void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            this._updateProgress.OnUpdateProgress(progress / limitTime, finger, isFingerDown, isIncreaseTime);
        }
        
        /// <summary>
        /// Sets the time limit for interaction.
        /// </summary>
        /// <param name="timeLimit">The time limit to set.</param>
        public void SetTimeLimit(float timeLimit)
        {
            this.limitTime = timeLimit;
        }
    }
}