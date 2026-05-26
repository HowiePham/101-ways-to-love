using Mimi.Analytics.Tracking.Trackers;

public struct WinViewFlowEventData : IEventData
{
    public enum EVENT_NAME
    {
        win_view_flow
    }

    public EVENT_NAME eventName { get; set; }
    public string level { get; set; }
    public string level_mode { get; set; }
    public string play_next_level { get; set; }
    public string return_home { get; set; }
    public string replay { get; set; }
    public string has_ads { get; set; }
    public int play_index { get; set; }
}