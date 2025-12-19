using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.MultipleTapping.Extensions
{
    [TypeInfoBox("Scales a target object when tapping is triggered.")]
    public class MultipleTappingScaleExtension : MonoMultipleTappingExtension
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Vector3 targetScale;
        [SerializeField] private float timeScale;
        [SerializeField] private bool isWaiting;

        /// <summary>
        /// Initializes the extension (no action required).
        /// </summary>
        public override void Init()
        {
        }

        /// <summary>
        /// Starts the extension (no action required).
        /// </summary>
        public override void Start()
        {
        }

        /// <summary>
        /// Scales target object and waits if configured.
        /// </summary>
        /// <param name="currentStep">The current tap step.</param>
        /// <param name="totalSteps">The total number of tap steps.</param>
        /// <returns>UniTask representing async completion.</returns>
        public override async UniTask OnTap(int currentStep, int totalSteps)
        {
            if (isWaiting)
            {
                await targetTransform.DOScale(targetScale, timeScale).AsyncWaitForCompletion().AsUniTask();
            }
            else
            {
                targetTransform.DOScale(targetScale, timeScale);
            }
        }
    }
}