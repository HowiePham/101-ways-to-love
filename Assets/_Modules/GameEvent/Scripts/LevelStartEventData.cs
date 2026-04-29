using Mimi.Analytics.Tracking.Trackers;

public struct LevelStartEventData : IEventData
{
    public enum EVENT_NAME
    {
        level_start
    }

    public EVENT_NAME eventName { get; set; }
    public string level { get; set; }
    public string level_mode { get; set; }
    public int life_count { get; set; }
}