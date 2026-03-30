using System;
using Cysharp.Threading.Tasks;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Games.Plugins;
using UnityEngine;

namespace Tracking
{
    public class LogAdCompletePlugin : IPlugin
    {
        private readonly IAdAdapter ads;
        private readonly IAnalyticTracker analyticTracker;

        private DateTime showTime;
        private string currentPlacement;
        private ImpressionData? currentImpression;
        private bool wasRewarded;

        public LogAdCompletePlugin(IAdAdapter ads, IAnalyticTracker analyticTracker)
        {
            this.ads = ads;
            this.analyticTracker = analyticTracker;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;

            // Interstitial
            this.ads.Interstitial.OnShowSucceeded += OnInterstitialShowSucceeded;
            this.ads.Interstitial.OnClosed += OnInterstitialClosed;
            this.ads.Interstitial.OnImpressionSuccess += OnInterstitialImpression;

            // Reward Video
            this.ads.RewardVideo.OnVideoOpened += OnRewardVideoOpened;
            this.ads.RewardVideo.OnRewarded += OnRewardVideoRewarded;
            this.ads.RewardVideo.OnVideoClosed += OnRewardVideoClosed;
            this.ads.RewardVideo.OnImpressionSuccess += OnRewardImpression;

            // App Open
            this.ads.AppOpen.OnShowSucceeded += OnAppOpenShowSucceeded;
            this.ads.AppOpen.OnClosed += OnAppOpenClosed;
            this.ads.AppOpen.OnImpressionSuccess += OnAppOpenImpression;
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;

            this.ads.Interstitial.OnShowSucceeded -= OnInterstitialShowSucceeded;
            this.ads.Interstitial.OnClosed -= OnInterstitialClosed;
            this.ads.Interstitial.OnImpressionSuccess -= OnInterstitialImpression;

            this.ads.RewardVideo.OnVideoOpened -= OnRewardVideoOpened;
            this.ads.RewardVideo.OnRewarded -= OnRewardVideoRewarded;
            this.ads.RewardVideo.OnVideoClosed -= OnRewardVideoClosed;
            this.ads.RewardVideo.OnImpressionSuccess -= OnRewardImpression;

            this.ads.AppOpen.OnShowSucceeded -= OnAppOpenShowSucceeded;
            this.ads.AppOpen.OnClosed -= OnAppOpenClosed;
            this.ads.AppOpen.OnImpressionSuccess -= OnAppOpenImpression;
        }

        public async UniTask Begin()
        {
            await UniTask.CompletedTask;
        }

        public async UniTask End()
        {
            await UniTask.CompletedTask;
        }

        // --- Interstitial ---

        private void OnInterstitialShowSucceeded(AdPlacement placement)
        {
            this.showTime = DateTime.UtcNow;
            this.currentPlacement = placement.location;
        }

        private void OnInterstitialImpression(ImpressionData impressionData)
        {
            this.currentImpression = impressionData;
        }

        private void OnInterstitialClosed(AdPlacement placement)
        {
            LogAdComplete("interstitial", "");
        }

        // --- Reward Video ---

        private void OnRewardVideoOpened(AdReward reward)
        {
            this.showTime = DateTime.UtcNow;
            this.currentPlacement = reward.RewardId;
            this.wasRewarded = false;
        }

        private void OnRewardImpression(ImpressionData impressionData)
        {
            this.currentImpression = impressionData;
        }

        private void OnRewardVideoRewarded(AdReward reward)
        {
            this.wasRewarded = true;
        }

        private void OnRewardVideoClosed(AdReward reward)
        {
            string endType = this.wasRewarded ? "done" : "quit";
            LogAdComplete("video_rewarded", endType);
        }

        // --- App Open ---

        private void OnAppOpenShowSucceeded(AdPlacement placement)
        {
            this.showTime = DateTime.UtcNow;
            this.currentPlacement = placement.location;
        }

        private void OnAppOpenImpression(ImpressionData impressionData)
        {
            this.currentImpression = impressionData;
        }

        private void OnAppOpenClosed(AdPlacement placement)
        {
            LogAdComplete("app_open", "");
        }

        // --- Common ---

        private void LogAdComplete(string adFormat, string endType)
        {
            var duration = (long)(DateTime.UtcNow - this.showTime).TotalMilliseconds;
            var impression = this.currentImpression;

            Debug.Log($"--- (TRACKING) Ad Complete - {adFormat}: {this.currentPlacement}, EndType: {endType}, Duration: {duration}ms");

            this.analyticTracker.LogEvent(new Feature_AD_COMPLETE
            {
                eventName = Feature_AD_COMPLETE.EVENT_NAME.ad_complete,
                ad_format = adFormat,
                ad_platform = impression?.AdPlatform.ToString() ?? "",
                ad_network = impression?.AdNetwork ?? "",
                end_type = endType,
                ad_duration = duration.ToString(),
                placement = this.currentPlacement ?? ""
            });

            this.currentImpression = null;
        }
    }
}
