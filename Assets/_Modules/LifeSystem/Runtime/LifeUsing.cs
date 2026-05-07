using Mimi.Events.AsyncBus;

public class LifeUsing : IMessage
{
    public LifeUsing(string reason)
    {
        this.Reason = reason;
    }

    public string Reason { private set; get; }
}