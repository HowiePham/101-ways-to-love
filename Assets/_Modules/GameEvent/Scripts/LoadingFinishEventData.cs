using Mimi.Analytics.Tracking.Trackers;

public struct LoadingFinishEventData : IEventData
{
    public enum EVENT_NAME
    {
        loading_finish
    }

    public EVENT_NAME eventName { get; set; }
    public string placement { get; set; }
    public string is_load { get; set; }
    public int load_time { get; set; }
    public string is_remote_config_loaded { get; set; }
}