using Mimi.Events.AsyncBus;

public class RecoveryLifeTimerUpdated : IMessage
{
    public RecoveryLifeTimerUpdated(string remainingTime)
    {
        this.RemainingTime = remainingTime;
    }

    public string RemainingTime { get; }
}