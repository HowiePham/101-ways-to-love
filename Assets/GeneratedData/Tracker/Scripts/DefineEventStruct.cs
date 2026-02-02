namespace Mimi.Analytics.Tracking.Trackers {
public struct Feature_AD_STATUS : IEventData{
	 public enum ACTION_NAME {
		NONE,
		 _ads_reward,
		 _force_ads,
		 _
}

	 public enum STATUS_RESULT {
		NONE,
		 _fail,
		 _succeed
}

	 public enum STATUS_INTERNET {
		NONE,
		 _yes,
		 _no
}

	 public enum EVENT_NAME {
		 ad_status}

	 public EVENT_NAME eventName { get; set; }
	 public ACTION_NAME action_name{ get; set; }
	 public string status_Ad_position{ get; set; }
	 public STATUS_RESULT status_result{ get; set; }
	 public STATUS_INTERNET status_internet{ get; set; }
}

public struct Feature_SESSION_START : IEventData{
	 public enum EVENT_NAME {
		 session_start}

	 public EVENT_NAME eventName { get; set; }
}

public struct Feature_LEVEL_START : IEventData{
	 public enum EVENT_NAME {
		 level_start}

	 public EVENT_NAME eventName { get; set; }
	 public string level{ get; set; }
	 public string level_mode{ get; set; }
}

public struct Feature_LEVEL_COMPLETE : IEventData{
	 public enum EVENT_NAME {
		 level_complete}

	 public EVENT_NAME eventName { get; set; }
	 public string level{ get; set; }
	 public string timeplayed{ get; set; }
}

public struct Feature_LEVEL_END : IEventData{
	 public enum EVENT_NAME {
		 level_end}

	 public EVENT_NAME eventName { get; set; }
	 public string level{ get; set; }
	 public string level_mode{ get; set; }
	 public string success{ get; set; }
}

public struct Feature_HINT : IEventData{
	 public enum EVENT_NAME {
		 hint}

	 public EVENT_NAME eventName { get; set; }
	 public string level{ get; set; }
}

public struct Feature_SKIP : IEventData{
	 public enum EVENT_NAME {
		 skip}

	 public EVENT_NAME eventName { get; set; }
	 public string level{ get; set; }
}

	}

