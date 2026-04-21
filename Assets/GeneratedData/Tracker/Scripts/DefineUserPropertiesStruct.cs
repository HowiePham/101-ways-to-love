namespace Mimi.Analytics.Tracking.Trackers {
	public enum USER_PROPERTIES_TYPE {
		current_level
	}
	public struct USER_PROPERTIES : IUserPropertyData {
		public USER_PROPERTIES_TYPE user_properties { get; set; }
		public string value { get; set; }
	}
	}

