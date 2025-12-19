using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

namespace Mimi.VisualActions.MultipleTapping.Extensions
{
    [TypeInfoBox("Plays a Spine animation when tapping is triggered.")]
    public class MultipleTappingPlayAnimationExtension : MonoMultipleTappingExtension
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField, SpineAnimation(dataField: "skeletonAnimation")] private string cuttingAnimationName;
        [SerializeField] private float speedAnim = 1f;
        [SerializeField] private bool isLoop;
        [SerializeField] private bool isWaiting;
        [SerializeField] private UnityEvent OnAnimationFinished;

        /// <summary>
        /// Initializes the extension (no action required).
        /// </summary>
        public override void Init()
        {
        }

        /// <summary>
        /// Sets animation timescale.
        /// </summary>
        public override void Start()
        {
            this.skeletonAnimation.timeScale = speedAnim;
        }

        /// <summary>
        /// Plays animation and waits if configured.
        /// </summary>
        /// <param name="currentStep">The current tap step.</param>
        /// <param name="totalSteps">The total number of tap steps.</param>
        /// <returns>UniTask representing async completion.</returns>
        public override async UniTask OnTap(int currentStep, int totalSteps)
        {
            var trackEntry = this.skeletonAnimation.AnimationState.SetAnimation(0, this.cuttingAnimationName, isLoop);
            trackEntry.Complete += OnAnimationEnd;
            if (!isLoop && isWaiting)
            {
                await UniTask.WaitUntil(() => this.skeletonAnimation.AnimationState.GetCurrent(0) == null ||
                                              this.skeletonAnimation.AnimationState.GetCurrent(0).IsComplete);
            }
        }

        /// <summary>
        /// Handles animation completion, invokes event.
        /// </summary>
        /// <param name="trackEntry">The completed animation track.</param>
        private void OnAnimationEnd(TrackEntry trackEntry)
        {
            OnAnimationFinished?.Invoke();
            trackEntry.Complete -= OnAnimationEnd;
        }
    }
}