using System;
using System.Threading;
using _Modules.GameEvent.Scripts;
using Cysharp.Threading.Tasks;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events;
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
        private readonly LifeSystem lifeSystem;

        private IDisposable levelStartedSub;
        private IDisposable levelCompletedSub;
        private IDisposable gamePausedSub;
        private IDisposable backHomeSub;
        private IDisposable adShowRequestedSub;
        private IDisposable actionFailedSub;

        private bool isInLevel;
        private bool isWatchingRewardVideo;
        private DateTime levelStartTime;
        private int falseCount;

        public LogLevelExitPlugin(RuntimeState runtimeState, IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker, IAdAdapter ads, LifeSystem lifeSystem)
        {
            this.runtimeState = runtimeState;
            this.eventSubscriber = eventSubscriber;
            this.analyticTracker = analyticTracker;
            this.ads = ads;
            this.lifeSystem = lifeSystem;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelStartedSub = this.eventSubscriber.Subscribe<LevelStarted>(LevelStartedHandler);
            this.levelCompletedSub = this.eventSubscriber.Subscribe<LevelCompleted>(LevelCompletedHandler);
            this.gamePausedSub = this.eventSubscriber.Subscribe<GamePaused>(GamePausedHandler);
            this.backHomeSub = this.eventSubscriber.Subscribe<BackHome>(BackHomeHandler);
            this.adShowRequestedSub = this.eventSubscriber.Subscribe<AdShowRequested>(AdShowRequestedHandler);
            this.actionFailedSub = this.eventSubscriber.Subscribe<ActionFailedMessage>(OnActionFailed);
            this.ads.Interstitial.OnShowSucceeded += OnInterShowed;
            this.ads.Interstitial.OnClosed += OnInterClosed;
            this.ads.Interstitial.OnShowFailed += OnInterShowFailed;
            this.ads.RewardVideo.OnVideoOpened += OnRewardVideoOpened;
            this.ads.RewardVideo.OnVideoClosed += OnRewardVideoClosed;
            this.ads.RewardVideo.OnShowFailed += OnRewardVideoShowFailed;
        }

        private void OnActionFailed(ActionFailedMessage actionFailedMessage)
        {
            this.falseCount++;
            Debug.Log($"--- (TRACKING)LogLevelExit Count false: {this.falseCount}");
        }


        private async UniTask LevelStartedHandler(LevelStarted levelStarted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.isInLevel = true;
            this.levelStartTime = DateTime.UtcNow;
            this.falseCount = 0;
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

            this.analyticTracker.LogEvent(new LevelExitEventData()
            {
                eventName = LevelExitEventData.EVENT_NAME.level_exit,
                level = currentLevelOrder.ToString(),
                mode = "normal",
                play_duration = (int)playDurationMs,
                false_count = this.falseCount,
                life_count = this.lifeSystem.CurrentLifeCount
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
            this.actionFailedSub.Dispose();
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