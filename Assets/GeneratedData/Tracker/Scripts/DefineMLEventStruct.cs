namespace Mimi.Analytics.Tracking.Trackers {
	public enum ML_EVENT_TYPE {
		level_complete_50,
		level_complete_100,
		ad_reward_3_times
	}
	public struct ML_EVENT : IMachineLearningEventData {
		public ML_EVENT_TYPE event_name { get; set; }
	}
	}

