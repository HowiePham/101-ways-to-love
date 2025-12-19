using System.Collections.Generic;
using CandyCoded.HapticFeedback;
using MEC;

namespace Mimi.Services.ScriptableObject.Vibration
{
    public class HapticVibrationService : IVibrationService
    {
        private bool enabled = true;
        private CoroutineHandle vibrateCoroutine;
        
        public HapticVibrationService()
        {
            this.enabled = true;
        }
        
        public void Play(long duration)
        {
            if (this.enabled)
            {
                Timing.KillCoroutines(this.vibrateCoroutine);
                this.vibrateCoroutine = default;
                if (this.vibrateCoroutine == default)
                {
                    this.vibrateCoroutine = Timing.RunCoroutine(VibrateCoroutine(duration));
                }
            }
        }

        public void PlayLoopDefault()
        {
            if (this.enabled)
                HapticFeedback.MediumFeedback();
        }

        public void PlayLoopStrong()
        {
            if (this.enabled)
                HapticFeedback.HeavyFeedback();
        }

        public void PlayLoopWeak()
        {
            if (this.enabled)
                HapticFeedback.LightFeedback();
        }

        public void Stop()
        {
        }

        public bool HasVibrator()
        {
            return this.enabled;
        }

        public void SetEnable(bool enable)
        {
            this.enabled = enable;
        }
        
        private IEnumerator<float> VibrateCoroutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                HapticFeedback.MediumFeedback();
                yield return Timing.WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
        }
    }
}