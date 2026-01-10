namespace Mimi.Analytics.Tracking.Trackers {
	public enum USER_PROPERTIES_TYPE {
		unlock_level,
		hard_currency_current,
		time_play
	}
	public struct USER_PROPERTIES : IUserPropertyData {
		public USER_PROPERTIES_TYPE user_properties { get; set; }
		public string value { get; set; }
	}
	}

