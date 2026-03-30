using Cysharp.Threading.Tasks;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Games.Plugins;
using UnityEngine;

namespace Tracking
{
    public class LogAdRequestPlugin : IPlugin
    {
        private readonly IAdAdapter ads;
        private readonly IAnalyticTracker analyticTracker;

        public LogAdRequestPlugin(IAdAdapter ads, IAnalyticTracker analyticTracker)
        {
            this.ads = ads;
            this.analyticTracker = analyticTracker;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.ads.Interstitial.OnLoadSucceeded += OnInterstitialLoadSucceeded;
            this.ads.Interstitial.OnLoadFailed += OnInterstitialLoadFailed;
            this.ads.RewardVideo.OnLoadSucceeded += OnRewardLoadSucceeded;
            this.ads.RewardVideo.OnLoadFailed += OnRewardLoadFailed;
            this.ads.Banner.OnLoadSucceeded += OnBannerLoadSucceeded;
            this.ads.Banner.OnLoadFailed += OnBannerLoadFailed;
            this.ads.Mrec.OnLoadSucceeded += OnMrecLoadSucceeded;
            this.ads.Mrec.OnLoadFailed += OnMrecLoadFailed;
            this.ads.AppOpen.OnLoadSucceeded += OnAppOpenLoadSucceeded;
            this.ads.AppOpen.OnLoadFailed += OnAppOpenLoadFailed;
        }

        private void OnInterstitialLoadSucceeded() => LogAdRequest("interstitial", "", "", 1);
        private void OnInterstitialLoadFailed(AdError error) => LogAdRequest("interstitial", "", "", 0);

        private void OnRewardLoadSucceeded() => LogAdRequest("video_rewarded", "", "", 1);
        private void OnRewardLoadFailed(AdError error) => LogAdRequest("video_rewarded", "", "", 0);

        private void OnBannerLoadSucceeded(AdPlacement placement) => LogAdRequest("banner", "", placement.location, 1);
        private void OnBannerLoadFailed(AdError error) => LogAdRequest("banner", "", "", 0);

        private void OnMrecLoadSucceeded() => LogAdRequest("mrec", "", "", 1);
        private void OnMrecLoadFailed(AdError error) => LogAdRequest("mrec", "", "", 0);

        private void OnAppOpenLoadSucceeded() => LogAdRequest("app_open", "", "", 1);
        private void OnAppOpenLoadFailed(AdError error) => LogAdRequest("app_open", "", "", 0);

        private void LogAdRequest(string adFormat, string adPlatform, string placement, int isLoad)
        {
            Debug.Log($"--- (TRACKING) Ad Request - {adFormat}: is_load={isLoad}");
            this.analyticTracker.LogEvent(new Feature_AD_REQUEST
            {
                eventName = Feature_AD_REQUEST.EVENT_NAME.ad_request,
                ad_format = adFormat,
                ad_platform = adPlatform,
                ad_network = "",
                placement = placement,
                is_load = isLoad,
                load_time = 0
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.ads.Interstitial.OnLoadSucceeded -= OnInterstitialLoadSucceeded;
            this.ads.Interstitial.OnLoadFailed -= OnInterstitialLoadFailed;
            this.ads.RewardVideo.OnLoadSucceeded -= OnRewardLoadSucceeded;
            this.ads.RewardVideo.OnLoadFailed -= OnRewardLoadFailed;
            this.ads.Banner.OnLoadSucceeded -= OnBannerLoadSucceeded;
            this.ads.Banner.OnLoadFailed -= OnBannerLoadFailed;
            this.ads.Mrec.OnLoadSucceeded -= OnMrecLoadSucceeded;
            this.ads.Mrec.OnLoadFailed -= OnMrecLoadFailed;
            this.ads.AppOpen.OnLoadSucceeded -= OnAppOpenLoadSucceeded;
            this.ads.AppOpen.OnLoadFailed -= OnAppOpenLoadFailed;
        }

        public async UniTask Begin()
        {
            await UniTask.CompletedTask;
        }

        public async UniTask End()
        {
            await UniTask.CompletedTask;
        }
    }
}
