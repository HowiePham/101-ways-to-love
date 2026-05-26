using Mimi.Analytics.Tracking.Trackers;

public struct ShowingAngelEventData : IEventData
{
    public enum EVENT_NAME
    {
        showing_angel
    }

    public EVENT_NAME eventName { get; set; }
    public string level { get; set; }
    public string level_mode { get; set; }
}