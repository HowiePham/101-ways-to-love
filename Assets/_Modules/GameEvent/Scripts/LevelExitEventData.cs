using Mimi.Analytics.Tracking.Trackers;

public struct LevelExitEventData : IEventData
{
    public enum EVENT_NAME
    {
        level_exit
    }

    public EVENT_NAME eventName { get; set; }
    public string level { get; set; }
    public string mode { get; set; }
    public int play_duration { get; set; }
    public int false_count { get; set; }
    public int life_count { get; set; }
}