using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events;
using _Modules.GameEvent.Scripts;
using Mimi.Events.AsyncBus;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

namespace Tracking
{
    public class LogLevelExitPlugin : IPlugin
    {
        private readonly RuntimeState runtimeState;
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAnalyticTracker analyticTracker;
        private readonly IAdAdapter ads;

        private IDisposable levelStartedSub;
        private IDisposable levelCompletedSub;
        private IDisposable gamePausedSub;
        private IDisposable backHomeSub;
        private IDisposable adShowRequestedSub;

        private bool isInLevel;
        private bool isWatchingRewardVideo;
        private DateTime levelStartTime;

        public LogLevelExitPlugin(RuntimeState runtimeState, IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker, IAdAdapter ads)
        {
            this.runtimeState = runtimeState;
            this.eventSubscriber = eventSubscriber;
            this.analyticTracker = analyticTracker;
            this.ads = ads;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelStartedSub = this.eventSubscriber.Subscribe<LevelStarted>(LevelStartedHandler);
            this.levelCompletedSub = this.eventSubscriber.Subscribe<LevelCompleted>(LevelCompletedHandler);
            this.gamePausedSub = this.eventSubscriber.Subscribe<GamePaused>(GamePausedHandler);
            this.backHomeSub = this.eventSubscriber.Subscribe<BackHome>(BackHomeHandler);
            this.adShowRequestedSub = this.eventSubscriber.Subscribe<AdShowRequested>(AdShowRequestedHandler);
            this.ads.Interstitial.OnShowSucceeded += OnInterShowed;
            this.ads.Interstitial.OnClosed += OnInterClosed;
            this.ads.Interstitial.OnShowFailed += OnInterShowFailed;
            this.ads.RewardVideo.OnVideoOpened += OnRewardVideoOpened;
            this.ads.RewardVideo.OnVideoClosed += OnRewardVideoClosed;
            this.ads.RewardVideo.OnShowFailed += OnRewardVideoShowFailed;
        }


        private async UniTask LevelStartedHandler(LevelStarted levelStarted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.isInLevel = true;
            this.levelStartTime = DateTime.UtcNow;
        }

        private async UniTask LevelCompletedHandler(LevelCompleted levelCompleted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.isInLevel = false;
        }

        private async UniTask AdShowRequestedHandler(AdShowRequested adShowRequested, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.isWatchingRewardVideo = true;
        }

        private async UniTask GamePausedHandler(GamePaused gamePaused, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            if (this.isWatchingRewardVideo) return;

            LogLevelExit();
        }

        private async UniTask BackHomeHandler(BackHome backHome, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            LogLevelExit();
        }

        private void OnInterShowed(AdPlacement adPlacement) => this.isWatchingRewardVideo = true;
        private void OnInterClosed(AdPlacement adPlacement) => this.isWatchingRewardVideo = false;
        private void OnInterShowFailed(AdError adError) => this.isWatchingRewardVideo = false;
        private void OnRewardVideoOpened(AdReward reward) => this.isWatchingRewardVideo = true;
        private void OnRewardVideoClosed(AdReward reward) => this.isWatchingRewardVideo = false;
        private void OnRewardVideoShowFailed(AdReward reward, AdError error) => this.isWatchingRewardVideo = false;

        private void LogLevelExit()
        {
            if (!this.isInLevel) return;
            if (this.isWatchingRewardVideo) return;

            this.isInLevel = false;
            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
            long playDurationMs = (long)(DateTime.UtcNow - this.levelStartTime).TotalMilliseconds;

            Debug.Log($"--- (TRACKING) Level Exit: {currentLevelOrder}, play_duration={playDurationMs}ms");
            this.analyticTracker.LogEvent(new Feature_LEVEL_EXIT
            {
                eventName = Feature_LEVEL_EXIT.EVENT_NAME.level_exit,
                level = currentLevelOrder.ToString(),
                mode = "normal",
                play_duration = playDurationMs.ToString()
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelStartedSub.Dispose();
            this.levelCompletedSub.Dispose();
            this.gamePausedSub.Dispose();
            this.backHomeSub.Dispose();
            this.adShowRequestedSub.Dispose();
            this.ads.Interstitial.OnShowSucceeded -= OnInterShowed;
            this.ads.Interstitial.OnClosed -= OnInterClosed;
            this.ads.Interstitial.OnShowFailed -= OnInterShowFailed;
            this.ads.RewardVideo.OnVideoOpened -= OnRewardVideoOpened;
            this.ads.RewardVideo.OnVideoClosed -= OnRewardVideoClosed;
            this.ads.RewardVideo.OnShowFailed -= OnRewardVideoShowFailed;
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