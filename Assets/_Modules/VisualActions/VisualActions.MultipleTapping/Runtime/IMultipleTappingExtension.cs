using System.Threading;
using Cysharp.Threading.Tasks;

namespace Mimi.VisualActions.MultipleTapping
{
    public interface IMultipleTappingExtension
    {
        /// <summary>
        /// Initializes the tapping extension.
        /// </summary>
        void Init();

        /// <summary>
        /// Starts the tapping extension.
        /// </summary>
        void Start();

        /// <summary>
        /// Handles tap event with current and total steps.
        /// </summary>
        /// <param name="currentStep">The current tap step.</param>
        /// <param name="totalSteps">The total number of tap steps.</param>
        /// <returns>UniTask representing async completion.</returns>
        UniTask OnTap(int currentStep, int totalSteps);
    }
}