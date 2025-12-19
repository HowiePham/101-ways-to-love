using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.MultipleTapping.Extensions
{
    [TypeInfoBox("Activates or deactivates the object when tapping is triggered.")]
    public class MultipleTappingSetActiveExtension : MonoMultipleTappingExtension
    {
        [SerializeField] private GameObject target;
        [SerializeField] private bool isActive;
        public override void Init()
        {
         
        }

        public override void Start()
        {
            
        }

        public override UniTask OnTap(int currentStep, int totalSteps)
        {
            target.SetActive(isActive);
            return UniTask.CompletedTask;
        }
    }
}