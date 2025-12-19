using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.MultipleTapping
{
    [TypeInfoBox("Handles multiple tapping within a specified area, triggering customizable extensions.")]
    public class MultipleTappingAction : VisualAction
    {
        [SerializeField] private int totalTimesOfTapping;
        [SerializeField] private MonoMultipleTappingExtension tappingExtension;
        [SerializeField] private bool isWaitingExtension;
        [SerializeField, InlineEditor(InlineEditorModes.FullEditor), Required]
        private BaseArea target;

        private bool complete;
        [ShowInInspector,ReadOnly] private int currentStep;
        [ShowInInspector,ReadOnly] private bool isTapping;
        public int CurrentStep => currentStep;

        /// <summary>
        /// Initializes the tapping extension.
        /// </summary>
        /// <returns>UniTask representing async completion.</returns>
        protected override UniTask OnInitializing()
        {
            tappingExtension.Init();
            return base.OnInitializing();
        }

        /// <summary>
        /// Starts the tapping extension.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel async operation.</param>
        /// <returns>UniTask representing async completion.</returns>
        protected override UniTask OnEnter(CancellationToken cancellationToken)
        {
            tappingExtension.Start();
            return base.OnEnter(cancellationToken);
        }

        /// <summary>
        /// Executes tapping process, tracking taps until completion.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel async operation.</param>
        /// <returns>UniTask representing async completion.</returns>
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                complete = false;
                currentStep = 0;
                LeanTouch.OnFingerTap += FingerTapHandler;
                await UniTask.WaitUntil(() => this.complete, PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                LeanTouch.OnFingerTap -= FingerTapHandler;
            }
        }
        
        /// <summary>
        /// Handles tap input, increments step, and triggers extension.
        /// </summary>
        /// <param name="finger">The LeanFinger input data.</param>
        private void FingerTapHandler(LeanFinger finger)
        {
            if (!this.target.ContainsScreenPosition(finger.ScreenPosition, Camera.main) || (isTapping && isWaitingExtension)) return;
            currentStep++;
            OnTapping();
            if (currentStep >= this.totalTimesOfTapping)
            {
                complete = true;
            }
        }

        /// <summary>
        /// Processes tap event, invoking extension.
        /// </summary>
        /// <returns>UniTask representing async completion.</returns>
        private async UniTask OnTapping()
        {
            isTapping = true;
            await tappingExtension.OnTap(currentStep, this.totalTimesOfTapping);
            isTapping = false;
        }
    }
}