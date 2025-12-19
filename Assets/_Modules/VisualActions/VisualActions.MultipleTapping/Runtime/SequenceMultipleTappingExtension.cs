using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.MultipleTapping
{
    [TypeInfoBox("Executes multiple tapping extensions sequentially.")]
    public class SequenceMultipleTappingExtension : MonoMultipleTappingExtension
    {
        [SerializeField] private MonoMultipleTappingExtension[] extensions;

        /// <summary>
        /// Initializes all child extensions.
        /// </summary>
        public override void Init()
        {
            foreach (var extension in extensions)
            {
                extension.Init();
            }
        }

        /// <summary>
        /// Starts all child extensions.
        /// </summary>
        public override void Start()
        {
            foreach (var extension in extensions)
            {
                extension.Start();
            }
        }

        /// <summary>
        /// Executes tap event sequentially for all extensions.
        /// </summary>
        /// <param name="currentStep">The current tap step.</param>
        /// <param name="totalSteps">The total number of tap steps.</param>
        /// <returns>UniTask representing async completion.</returns>
        public override async UniTask OnTap(int currentStep, int totalSteps)
        {
            try
            {
                foreach (var extension in extensions)
                {
                    await extension.OnTap(currentStep, totalSteps);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}