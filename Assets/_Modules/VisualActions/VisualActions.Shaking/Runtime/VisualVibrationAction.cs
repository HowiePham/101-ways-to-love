using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Services.ScriptableObject.Vibration;
using UnityEngine;

namespace Mimi.VisualActions.Shaking
{
    public class VisualVibrationAction : VisualAction
    {
        [SerializeField] private bool isSingleVibration;
        [SerializeField] private long duration;
        [SerializeField] private BaseVibrationServiceSO vibrationService;
        protected override UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (isSingleVibration)
            {
                vibrationService.PlayLoopDefault();
            }
            else
            {
                vibrationService.Play(duration);
            }

            return UniTask.CompletedTask;
        }
    }
}