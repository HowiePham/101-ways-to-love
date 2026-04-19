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
	 public string result{ get; set; }
	 public string use_hint{ get; set; }
	 public string use_skip{ get; set; }
	 public string play_duration{ get; set; }
	 public string false_count{ get; set; }
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

public struct Feature_AD_CLICKED : IEventData{
	 public enum EVENT_NAME {
		 ad_clicked}

	 public EVENT_NAME eventName { get; set; }
	 public string ad_format{ get; set; }
	 public string ad_platform{ get; set; }
	 public string ad_network{ get; set; }
	 public string placement{ get; set; }
}

public struct Feature_AD_COMPLETE : IEventData{
	 public enum EVENT_NAME {
		 ad_complete}

	 public EVENT_NAME eventName { get; set; }
	 public string ad_format{ get; set; }
	 public string ad_platform{ get; set; }
	 public string ad_network{ get; set; }
	 public string end_type{ get; set; }
	 public string ad_duration{ get; set; }
	 public string placement{ get; set; }
}

public struct Feature_AD_REQUEST : IEventData{
	 public enum EVENT_NAME {
		 ad_request}

	 public EVENT_NAME eventName { get; set; }
	 public string ad_format{ get; set; }
	 public string ad_platform{ get; set; }
	 public string ad_network{ get; set; }
	 public string placement{ get; set; }
	 public string is_load{ get; set; }
	 public string load_time{ get; set; }
}

public struct Feature_LEVEL_EXIT : IEventData{
	 public enum EVENT_NAME {
		 level_exit}

	 public EVENT_NAME eventName { get; set; }
	 public string level{ get; set; }
	 public string mode{ get; set; }
	 public string play_duration{ get; set; }
	 public string false_count{ get; set; }
}

public struct Feature_LEVEL_REOPEN : IEventData{
	 public enum EVENT_NAME {
		 level_reopen}

	 public EVENT_NAME eventName { get; set; }
	 public string level{ get; set; }
	 public string mode{ get; set; }
}

public struct Feature_RESOURCE_EARN : IEventData{
	 public enum EVENT_NAME {
		 resource_earn}

	 public EVENT_NAME eventName { get; set; }
	 public string resource_type{ get; set; }
	 public string resource_name{ get; set; }
	 public string resource_amount{ get; set; }
	 public string placement{ get; set; }
	 public string resource_balance{ get; set; }
}

public struct Feature_RESOURCE_SPEND : IEventData{
	 public enum EVENT_NAME {
		 resource_spend}

	 public EVENT_NAME eventName { get; set; }
	 public string resource_type{ get; set; }
	 public string resource_name{ get; set; }
	 public string resource_amount{ get; set; }
	 public string placement{ get; set; }
	 public string resource_balance{ get; set; }
}

public struct Feature_IAP_SHOW : IEventData{
	 public enum EVENT_NAME {
		 iap_show}

	 public EVENT_NAME eventName { get; set; }
	 public string placement{ get; set; }
	 public string show_type{ get; set; }
	 public string trigger_type{ get; set; }
	 public string pack_name{ get; set; }
}

public struct Feature_IAP_CLICK : IEventData{
	 public enum EVENT_NAME {
		 iap_click}

	 public EVENT_NAME eventName { get; set; }
	 public string placement{ get; set; }
	 public string show_type{ get; set; }
	 public string trigger_type{ get; set; }
	 public string pack_name{ get; set; }
}

public struct Feature_IAP_PURCHASE : IEventData{
	 public enum EVENT_NAME {
		 iap_purchase}

	 public EVENT_NAME eventName { get; set; }
	 public string placement{ get; set; }
	 public string show_type{ get; set; }
	 public string trigger_type{ get; set; }
	 public string pack_name{ get; set; }
	 public string price{ get; set; }
	 public string currency{ get; set; }
}

	}

