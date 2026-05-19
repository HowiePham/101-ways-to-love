using Mimi.Analytics.Tracking.Trackers;

public struct SessionEndEventData : IEventData
{
    public enum EVENT_NAME
    {
        session_end
    }

    public EVENT_NAME eventName { get; set; }
    public string level { get; set; }
    public string last_screen_name { get; set; }
    public int life_count { get; set; }
    public int duration { get; set; }
    public int total_played_level { get; set; }
}