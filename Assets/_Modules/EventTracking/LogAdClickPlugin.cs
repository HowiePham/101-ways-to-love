using Cysharp.Threading.Tasks;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Games.Plugins;
using UnityEngine;

namespace Tracking
{
    public class LogAdClickPlugin : IPlugin
    {
        private readonly IAdAdapter ads;
        private readonly IAnalyticTracker analyticTracker;
        private ImpressionData lastFullscreenImpression;
        private ImpressionData lastBannerImpression;
        private ImpressionData lastMrecImpression;

        public LogAdClickPlugin(IAdAdapter ads, IAnalyticTracker analyticTracker)
        {
            this.ads = ads;
            this.analyticTracker = analyticTracker;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.ads.Interstitial.OnImpressionSuccess += OnFullscreenImpression;
            this.ads.Interstitial.OnClicked += OnInterstitialClicked;
            this.ads.RewardVideo.OnImpressionSuccess += OnFullscreenImpression;
            this.ads.RewardVideo.OnVideoClicked += OnRewardVideoClicked;
            this.ads.Banner.OnImpressionSuccess += OnBannerImpression;
            this.ads.Banner.OnClicked += OnBannerClicked;
            this.ads.Mrec.OnImpressionSuccess += OnMrecImpression;
            this.ads.Mrec.OnClicked += OnMrecClicked;
            this.ads.AppOpen.OnImpressionSuccess += OnFullscreenImpression;
            this.ads.AppOpen.OnClicked += OnAppOpenClicked;
        }

        private void OnFullscreenImpression(ImpressionData data) => this.lastFullscreenImpression = data;
        private void OnBannerImpression(ImpressionData data) => this.lastBannerImpression = data;
        private void OnMrecImpression(ImpressionData data) => this.lastMrecImpression = data;

        private void OnInterstitialClicked(AdPlacement placement)
        {
            LogAdClick("interstitial", this.lastFullscreenImpression, placement.location);
        }

        private void OnRewardVideoClicked(AdReward reward)
        {
            LogAdClick("video_rewarded", this.lastFullscreenImpression, this.lastFullscreenImpression.InGamePlacement.location);
        }

        private void OnBannerClicked(AdPlacement placement)
        {
            LogAdClick("banner", this.lastBannerImpression, placement.location);
        }

        private void OnMrecClicked(AdPlacement placement)
        {
            LogAdClick("mrec", this.lastMrecImpression, placement.location);
        }

        private void OnAppOpenClicked(AdPlacement placement)
        {
            LogAdClick("app_open", this.lastFullscreenImpression, placement.location);
        }

        private void LogAdClick(string adFormat, ImpressionData impression, string placement)
        {
            Debug.Log($"--- (TRACKING) Ad Click - {adFormat}: {placement}");
            this.analyticTracker.LogEvent(new Feature_AD_CLICK
            {
                eventName = Feature_AD_CLICK.EVENT_NAME.ad_click,
                ad_format = adFormat,
                ad_platform = impression.AdPlatform.ToString(),
                ad_network = impression.AdNetwork ?? "",
                placement = placement ?? ""
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.ads.Interstitial.OnImpressionSuccess -= OnFullscreenImpression;
            this.ads.Interstitial.OnClicked -= OnInterstitialClicked;
            this.ads.RewardVideo.OnImpressionSuccess -= OnFullscreenImpression;
            this.ads.RewardVideo.OnVideoClicked -= OnRewardVideoClicked;
            this.ads.Banner.OnImpressionSuccess -= OnBannerImpression;
            this.ads.Banner.OnClicked -= OnBannerClicked;
            this.ads.Mrec.OnImpressionSuccess -= OnMrecImpression;
            this.ads.Mrec.OnClicked -= OnMrecClicked;
            this.ads.AppOpen.OnImpressionSuccess -= OnFullscreenImpression;
            this.ads.AppOpen.OnClicked -= OnAppOpenClicked;
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
