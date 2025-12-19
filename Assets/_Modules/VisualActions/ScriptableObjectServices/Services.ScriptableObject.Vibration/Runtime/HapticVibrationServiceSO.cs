using UnityEngine;

namespace Mimi.Services.ScriptableObject.Vibration
{
    [CreateAssetMenu(menuName = "ServicesSO/Vibration/HapticVibrationServiceSO")]
    public class HapticVibrationServiceSO : BaseVibrationServiceSO
    {
        public override void Dispose()
        {
            
        }

        protected override IVibrationService CreateVibrationPlayer()
        {
            return new HapticVibrationService();
        }
    }
}