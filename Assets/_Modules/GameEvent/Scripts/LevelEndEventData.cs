using Mimi.Analytics.Tracking.Trackers;

public struct LevelEndEventData : IEventData
{
    public enum EVENT_NAME
    {
        level_end
    }

    public EVENT_NAME eventName { get; set; }
    public string level { get; set; }
    public string level_mode { get; set; }
    public string result { get; set; }
    public string use_hint { get; set; }
    public string use_skip { get; set; }
    public int play_duration { get; set; }
    public int false_count { get; set; }
    public int life_count { get; set; }
    public int play_index { get; set; }
    public int win_index { get; set; }
}