namespace Mimi.Services.ScriptableObject.Vibration
{
    public interface IVibrationService 
    {
        void Play(long duration);
        void PlayLoopDefault();
        void PlayLoopStrong();
        void PlayLoopWeak();
        void Stop();
        bool HasVibrator();
        void SetEnable(bool enable);
    }
}