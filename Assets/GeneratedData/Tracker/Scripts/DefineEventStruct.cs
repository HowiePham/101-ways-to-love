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

	}

