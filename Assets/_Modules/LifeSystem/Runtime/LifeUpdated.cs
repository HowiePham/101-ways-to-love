using Mimi.Events.AsyncBus;

public class LifeUpdated : IMessage
{
    public int LifeCount { get; }
    public string RemainingTime { get; }

    public LifeUpdated(int lifeCount, string remainingTime)
    {
        this.LifeCount = lifeCount;
        this.RemainingTime = remainingTime;
    }
}