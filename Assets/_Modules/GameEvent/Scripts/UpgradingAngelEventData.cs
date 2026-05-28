using Mimi.Analytics.Tracking.Trackers;

public struct UpgradingAngelEventData : IEventData
{
    public enum EVENT_NAME
    {
        upgrading_angel
    }

    public EVENT_NAME eventName { get; set; }
    public string level { get; set; }
    public string level_mode { get; set; }
}