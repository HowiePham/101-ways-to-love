using Cysharp.Threading.Tasks;
using Mimi.Services.ScriptableObject.Core;

namespace Mimi.Services.ScriptableObject.Vibration
{
    public abstract class BaseVibrationServiceSO : BaseServiceSO, IVibrationService
    {
        protected IVibrationService wrapVibrationPlayer;

        protected virtual IVibrationService WrapVibrationPlayer
        {
            get
            {
                if (wrapVibrationPlayer == null)
                {
                    Initialize();
                }

                return wrapVibrationPlayer;
            }
        }

        public override UniTask Initialize()
        {
            wrapVibrationPlayer = CreateVibrationPlayer();
            return UniTask.CompletedTask;
        }

        protected abstract IVibrationService CreateVibrationPlayer();
        public void Play(long duration)
        {
            WrapVibrationPlayer?.Play(duration);
        }

        public void PlayLoopDefault()
        {
            WrapVibrationPlayer?.PlayLoopDefault();
        }

        public void PlayLoopStrong()
        {
           WrapVibrationPlayer?.PlayLoopStrong();
        }

        public void PlayLoopWeak()
        {
            WrapVibrationPlayer?.PlayLoopWeak();
        }

        public void Stop()
        {
            WrapVibrationPlayer?.Stop();
        }

        public bool HasVibrator()
        {
            if (WrapVibrationPlayer == null) return false;
            return WrapVibrationPlayer.HasVibrator();
        }

        public void SetEnable(bool enable)
        {
            WrapVibrationPlayer?.SetEnable(enable);
        }
    }
}