using System;
using System.Threading;
using _Modules.GameEvent.Scripts;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

namespace Tracking
{
    public class LogSessionDurationPlugin : IPlugin
    {
        private readonly RuntimeState runtimeState;
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAnalyticTracker analyticTracker;
        private readonly LifeSystem lifeSystem;
        private readonly IAdAdapter ads;

        private IDisposable bootCompletedSub;
        private IDisposable levelStartedSub;
        private IDisposable gamePausedSub;
        private IDisposable gameUnpausedSub;
        private IDisposable gameExitedSub;
        private IDisposable screenShownSub;
        private IDisposable adShowRequestedSub;

        private DateTime sessionStartTime;
        private bool hasSessionStarted;
        private bool isWatchingAd;
        private int totalLevelsPlayedInSession;
        private string lastScreenName = "unknown";

        public LogSessionDurationPlugin(
            RuntimeState runtimeState,
            IAsyncSubscriber eventSubscriber,
            IAnalyticTracker analyticTracker,
            LifeSystem lifeSystem,
            IAdAdapter ads)
        {
            this.runtimeState = runtimeState;
            this.eventSubscriber = eventSubscriber;
            this.analyticTracker = analyticTracker;
            this.lifeSystem = lifeSystem;
            this.ads = ads;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.bootCompletedSub = this.eventSubscriber.Subscribe<BootGameCompleted>(OnBootCompleted);
            this.levelStartedSub  = this.eventSubscriber.Subscribe<LevelStarted>(OnLevelStarted);
            this.gamePausedSub    = this.eventSubscriber.Subscribe<GamePaused>(OnGamePaused);
            this.gameUnpausedSub = this.eventSubscriber.Subscribe<GameUnpaused>(OnGameUnpaused);
            this.gameExitedSub = this.eventSubscriber.Subscribe<GameExited>(OnGameExited);
            this.screenShownSub = this.eventSubscriber.Subscribe<ScreenShown>(OnScreenShown);
            this.adShowRequestedSub = this.eventSubscriber.Subscribe<AdShowRequested>(OnAdShowRequested);
            this.ads.Interstitial.OnShowSucceeded += OnAdOpened;
            this.ads.Interstitial.OnClicked += OnAdClicked;
            this.ads.Interstitial.OnClosed += OnInterClosed;
            this.ads.Interstitial.OnShowFailed += OnAdShowFailed;
            this.ads.RewardVideo.OnVideoOpened += OnRewardOpened;
            this.ads.RewardVideo.OnVideoClicked += OnRewardVideoClicked;
            this.ads.RewardVideo.OnVideoClosed += OnRewardClosed;
            this.ads.RewardVideo.OnShowFailed += OnRewardShowFailed;
            this.ads.Banner.OnClicked += OnAdClicked;
            this.ads.Mrec.OnClicked += OnAdClicked;
            this.ads.AppOpen.OnClicked += OnAdClicked;

            DebugLogConsole.AddCommandInstance("log-session-duration", "Log session duration", "FireSessionEnd", this);
        }

        private async UniTask OnBootCompleted(BootGameCompleted msg, CancellationToken ct)
        {
            await UniTask.CompletedTask;
            this.sessionStartTime = DateTime.UtcNow;
            this.hasSessionStarted = true;
            Debug.Log("--- (TRACKING) LogSessionDuration: timer started");
        }

        private async UniTask OnLevelStarted(LevelStarted msg, CancellationToken ct)
        {
            await UniTask.CompletedTask;
            this.totalLevelsPlayedInSession++;
        }

        private async UniTask OnScreenShown(ScreenShown msg, CancellationToken ct)
        {
            await UniTask.CompletedTask;
            this.lastScreenName = msg.ScreenName;
        }

        private async UniTask OnAdShowRequested(AdShowRequested msg, CancellationToken ct)
        {
            await UniTask.CompletedTask;
            this.isWatchingAd = true;
        }

        private async UniTask OnGamePaused(GamePaused msg, CancellationToken ct)
        {
            await UniTask.CompletedTask;
            if (this.isWatchingAd) return;
            FireSessionEnd();
        }

        private async UniTask OnGameExited(GameExited msg, CancellationToken ct)
        {
            await UniTask.CompletedTask;
            FireSessionEnd();
        }

        private async UniTask OnGameUnpaused(GameUnpaused msg, CancellationToken ct)
        {
            await UniTask.CompletedTask;
            this.isWatchingAd = false;
        }

        private void OnAdOpened(AdPlacement adPlacement) => this.isWatchingAd = true;
        private void OnAdClicked(AdPlacement adPlacement) => this.isWatchingAd = true;
        private void OnInterClosed(AdPlacement adPlacement) => this.isWatchingAd = false;
        private void OnAdShowFailed(AdError adError) => this.isWatchingAd = false;
        private void OnRewardOpened(AdReward adReward) => this.isWatchingAd = true;
        private void OnRewardVideoClicked(AdReward adReward) => this.isWatchingAd = true;
        private void OnRewardClosed(AdReward adReward) => this.isWatchingAd = false;
        private void OnRewardShowFailed(AdReward adReward, AdError adError) => this.isWatchingAd = false;

        private void FireSessionEnd()
        {
            if (!this.hasSessionStarted) return;

            int level = this.runtimeState.CurrentLevelOrder.Value + 1;
            int totalPlayedLevel = this.totalLevelsPlayedInSession;
            int durationMs = (int)(DateTime.UtcNow - this.sessionStartTime).TotalMilliseconds;
            int lifeCount = this.lifeSystem.CurrentLifeCount;

            Debug.Log($"--- (TRACKING) LogSessionDuration: level={level}, screen={this.lastScreenName}, life={lifeCount}, duration={durationMs}ms, total_played={totalPlayedLevel}");

            this.analyticTracker.LogEvent(new SessionEndEventData()
            {
                eventName = SessionEndEventData.EVENT_NAME.session_end,
                level = level.ToString(),
                last_screen_name = this.lastScreenName,
                life_count = lifeCount,
                session_duration = durationMs,
                total_played_level = totalPlayedLevel,
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.bootCompletedSub?.Dispose();
            this.levelStartedSub?.Dispose();
            this.gamePausedSub?.Dispose();
            this.gameUnpausedSub?.Dispose();
            this.gameExitedSub?.Dispose();
            this.screenShownSub?.Dispose();
            this.adShowRequestedSub?.Dispose();
            this.ads.Interstitial.OnShowSucceeded -= OnAdOpened;
            this.ads.Interstitial.OnClicked -= OnAdClicked;
            this.ads.Interstitial.OnClosed -= OnInterClosed;
            this.ads.Interstitial.OnShowFailed -= OnAdShowFailed;
            this.ads.RewardVideo.OnVideoOpened -= OnRewardOpened;
            this.ads.RewardVideo.OnVideoClicked -= OnRewardVideoClicked;
            this.ads.RewardVideo.OnVideoClosed -= OnRewardClosed;
            this.ads.RewardVideo.OnShowFailed -= OnRewardShowFailed;
            this.ads.Banner.OnClicked -= OnAdClicked;
            this.ads.Mrec.OnClicked -= OnAdClicked;
            this.ads.AppOpen.OnClicked -= OnAdClicked;
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