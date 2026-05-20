using Mimi.Analytics.Tracking.Trackers;

public struct ChapterRewardEventData : IEventData
{
    public enum EVENT_NAME
    {
        chapter_reward
    }

    public EVENT_NAME eventName { get; set; }
    public string level { get; set; }
    public string use_reward_bonus { get; set; }
}