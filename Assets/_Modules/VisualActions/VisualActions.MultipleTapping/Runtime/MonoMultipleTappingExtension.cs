using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mimi.VisualActions.MultipleTapping
{
    public abstract class MonoMultipleTappingExtension : MonoBehaviour, IMultipleTappingExtension
    {
        public abstract void Init();
        public abstract void Start();
        public abstract UniTask OnTap(int currentStep, int totalSteps);
    }
}