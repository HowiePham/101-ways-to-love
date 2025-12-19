using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.MultipleTapping.Extensions
{
    [TypeInfoBox("Moves a target object based on tapping progress.")]
    public class MultipleTapMovingExtension : MonoMultipleTappingExtension
    {
        [SerializeField] private Transform targetObject;
        [SerializeField] private Transform startPos;
        [SerializeField] private Transform endPos;
        [SerializeField] private float timeMovingBetweenSteps;
        [SerializeField] private bool isWaiting;

        /// <summary>
        /// Initializes the extension (no action required).
        /// </summary>
        public override void Init()
        {
        }

        /// <summary>
        /// Sets target object to start position.
        /// </summary>
        public override void Start()
        {
            targetObject.transform.position = startPos.position;
        }

        /// <summary>
        /// Moves target object incrementally based on tap progress.
        /// </summary>
        /// <param name="currentStep">The current tap step.</param>
        /// <param name="totalSteps">The total number of tap steps.</param>
        /// <returns>UniTask representing async completion.</returns>
        public override async UniTask OnTap(int currentStep, int totalSteps)
        {
            Vector3 distance = (endPos.position - startPos.position) / totalSteps;
            if (isWaiting)
            {
                await targetObject.DOMove(targetObject.position + distance, this.timeMovingBetweenSteps).AsyncWaitForCompletion().AsUniTask();
            }
            else
            {
                targetObject.DOMove(targetObject.position + distance, this.timeMovingBetweenSteps);
            }
        }
    }
}