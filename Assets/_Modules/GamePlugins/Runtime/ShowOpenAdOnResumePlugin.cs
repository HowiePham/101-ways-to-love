using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Sessions;
using Mimi.Configs;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Games.Plugins;
using UnityEngine;

namespace Ads
{
    public class ShowOpenAdOnResumePlugin : IPlugin
    {
        private readonly IAdAdapter adAdapter;
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IConfigProvider remoteConfig;
        private bool isRemoveAds;
        private bool isFirstSession;
        private IDisposable gameResumeEventHandler;

        public bool resumeFromAds = false;
        private bool isBootCompleted;

        public ShowOpenAdOnResumePlugin(IAdAdapter adAdapter, IAsyncSubscriber eventSubscriber, bool isRemoveAds , IConfigProvider remoteConfig, bool isFirstSession)
        {
            this.adAdapter = adAdapter;
            this.eventSubscriber = eventSubscriber;
            this.isRemoveAds = isRemoveAds;
            this.remoteConfig = remoteConfig;
            this.isFirstSession = isFirstSession;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            Debug.Log($"--- (PLUGIN) Installing ShowOpenAdOnResumePlugin...");

            this.adAdapter.Interstitial.OnShowSucceeded += InterstitialShowSuccessHandler;
            this.adAdapter.RewardVideo.OnVideoOpened += RewardVideoShowSuccessHandler;
            this.adAdapter.Banner.OnClicked += BannerClickHandler;
            this.adAdapter.Mrec.OnClicked += MrecClickHandler;
            this.adAdapter.AppOpen.OnShowSucceeded += AdShownHandler;

            this.gameResumeEventHandler = this.eventSubscriber.Subscribe<BootGameCompleted>(BootGameCompletedHandler);
            this.gameResumeEventHandler = this.eventSubscriber.Subscribe<GameUnpaused>(GameResumedHandler);
        }

        private async UniTask BootGameCompletedHandler(BootGameCompleted bootGameCompleted,
            CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.isBootCompleted = true;
        }

        private void MrecClickHandler(AdPlacement obj)
        {
            this.resumeFromAds = true;
        }


        private void BannerClickHandler(AdPlacement placement)
        {
            this.resumeFromAds = true;
        }

        private void AdShownHandler(AdPlacement placement)
        {
            this.resumeFromAds = true;
        }

        private void RewardVideoShowSuccessHandler(AdReward rewardData)
        {
            this.resumeFromAds = true;
        }

        private void InterstitialShowSuccessHandler(AdPlacement placement)
        {
            this.resumeFromAds = true;
        }

        private async UniTask GameResumedHandler(GameUnpaused gameResumed, CancellationToken cancellationToken)
        {
            Debug.Log($"--- (PLUGIN) Resume Ads handling...");
            await UniTask.WaitForEndOfFrame(cancellationToken);
            await UniTask.Delay(400, cancellationToken: cancellationToken);
            if (!this.isFirstSession && !this.isRemoveAds && this.isBootCompleted)
            {
                ShowResumeAds();
            }

            this.resumeFromAds = false;
        }

        private void ShowResumeAds()
        {
            if (this.remoteConfig.GetValue(ConfigKey.ResumeAds).Boolean &&
                !this.resumeFromAds)
            {
                this.adAdapter.AppOpen.Show(new AdPlacement("resume_app"));
            }
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.gameResumeEventHandler.Dispose();
            this.adAdapter.Interstitial.OnShowSucceeded -= InterstitialShowSuccessHandler;
            this.adAdapter.RewardVideo.OnVideoOpened -= RewardVideoShowSuccessHandler;
            this.adAdapter.Banner.OnClicked -= BannerClickHandler;
            this.adAdapter.Mrec.OnClicked -= MrecClickHandler;
            this.adAdapter.AppOpen.OnShowSucceeded -= AdShownHandler;
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