using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class TimingBar : MonoBehaviour
{
    [SerializeField] protected float cycleTime = 1.5f;
    [SerializeField] protected float minCycleTime = 0.1f;
    [SerializeField] protected RectTransform trueArea;
    [SerializeField] protected RectTransform timingBarContainer;
    protected bool isRunning;

    [Button]
    public abstract UniTask Show();

    [Button]
    public abstract UniTask Hide();

    [Button]
    public abstract void StartRunning();

    [Button]
    public abstract void ResumeRunning();

    [Button]
    public abstract void StopRunning();

    [Button]
    public abstract void TapTimingBar();

    [Button]
    public abstract void ResetBar();

    [Button]
    public abstract void RandomTrueArea();

    public abstract bool IsTrueTiming();

    public void IncreaseRunningSpeed(float value)
    {
        float newValue = this.cycleTime - value;
        if (newValue <= this.minCycleTime)
        {
            newValue = this.minCycleTime;
        }

        this.cycleTime = newValue;
    }

    public abstract void DecreaseTrueAreaWidth(float value);
}