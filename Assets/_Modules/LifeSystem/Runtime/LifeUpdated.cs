using Mimi.Events.AsyncBus;

public class LifeUpdated : IMessage
{
    public int LifeCount { get; }

    public LifeUpdated(int lifeCount)
    {
        this.LifeCount = lifeCount;
    }
}