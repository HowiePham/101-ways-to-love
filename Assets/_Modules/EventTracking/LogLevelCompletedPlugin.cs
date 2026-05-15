using System;
using System.Threading;
using _Modules.GameEvent.Scripts;
using Cysharp.Threading.Tasks;
using FrogunnerGames;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Games.Events;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using UnityEngine;

namespace Tracking
{
    public class LogLevelCompletedPlugin : IPlugin
    {
        private GameContext gameContext;
        private readonly RuntimeState runtimeState;
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAnalyticTracker analyticTracker;
        private readonly IAdAdapter ads;
        private readonly LifeSystem lifeSystem;

        private IDisposable levelCompletedSub;
        private IDisposable levelStartSub;
        private IDisposable levelSkipSub;
        private IDisposable levelHintSub;
        private IDisposable actionFailedSub;
        private bool useSkip;
        private bool useHint;
        private DateTime startTime;
        private DateTime adStartTime;
        private long totalAdDurationMs;
        private int falseCount;

        public LogLevelCompletedPlugin(RuntimeState runtimeState, IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker, IAdAdapter ads, LifeSystem lifeSystem)
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
            this.levelCompletedSub = this.eventSubscriber.Subscribe<LevelCompleted>(LevelCompletedHandler);
            this.levelStartSub = this.eventSubscriber.Subscribe<LevelStarted>(LevelStartedHandler);
            this.levelSkipSub = this.eventSubscriber.Subscribe<SkipLevel>(SkipHandler);
            this.levelHintSub = this.eventSubscriber.Subscribe<UseHint>(HintHandler);
            this.actionFailedSub = this.eventSubscriber.Subscribe<ActionFailedMessage>(OnActionFailed);
            this.ads.Interstitial.OnShowSucceeded += OnAdOpened;
            this.ads.Interstitial.OnClosed += OnAdClosed;
            this.ads.RewardVideo.OnVideoOpened += OnRewardOpened;
            this.ads.RewardVideo.OnVideoClosed += OnRewardClosed;
        }

        private void OnActionFailed(ActionFailedMessage actionFailedMessage)
        {
            this.falseCount++;
            Debug.Log($"--- (TRACKING) LogLevelCompleted Count false: {this.falseCount}");
        }

        private async UniTask SkipHandler(SkipLevel skipLevel, CancellationToken cancellation)
        {
            await UniTask.CompletedTask;
            this.useSkip = true;
        }

        private async UniTask HintHandler(UseHint useHint, CancellationToken cancellation)
        {
            await UniTask.CompletedTask;
            this.useHint = true;
        }

        private void OnAdOpened(AdPlacement adPlacement)
        {
            this.adStartTime = DateTime.UtcNow;
        }

        private void OnAdClosed(AdPlacement adPlacement)
        {
            this.totalAdDurationMs += (long)(DateTime.UtcNow - this.adStartTime).TotalMilliseconds;
        }

        private void OnRewardOpened(AdReward reward)
        {
            this.adStartTime = DateTime.UtcNow;
        }

        private void OnRewardClosed(AdReward reward)
        {
            this.totalAdDurationMs += (long)(DateTime.UtcNow - this.adStartTime).TotalMilliseconds;
        }

        private async UniTask LevelStartedHandler(LevelStarted levelStarted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.useHint = false;
            this.useSkip = false;
            this.startTime = DateTime.UtcNow;
            this.totalAdDurationMs = 0;
            this.falseCount = 0;
        }

        private async UniTask LevelCompletedHandler(LevelCompleted levelCompleted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
            int playIndex = PlayerPrefs.GetInt($"play_index_{currentLevelOrder}", 0);
            int winIndex = PlayerPrefs.GetInt($"win_index_{currentLevelOrder}", 0);
            long playDurationMs = (long)(DateTime.UtcNow - this.startTime).TotalMilliseconds - this.totalAdDurationMs;
            LevelCompletionStatus levelCompletionStatus = levelCompleted.Status;

            if (levelCompletionStatus == LevelCompletionStatus.Win)
            {
                winIndex++;
                PlayerPrefs.SetInt($"win_index_{currentLevelOrder}", winIndex);
            }

            Debug.Log(
                $"--- (TRACKING) Log level Completed: {currentLevelOrder} " +
                $"--- Hint: {this.useHint} " +
                $"--- Skip: {this.useSkip} " +
                $"--- False: {this.falseCount} " +
                $"--- Life: {this.lifeSystem.CurrentLifeCount} " +
                $"--- Duration: {playDurationMs}ms " +
                $"--- PlayIndex: {playIndex} " +
                $"--- WinIndex: {winIndex}");

            {
                this.analyticTracker.LogEvent(new LevelEndEventData()
                {
                    eventName = LevelEndEventData.EVENT_NAME.level_end,
                    level = currentLevelOrder.ToString(),
                    level_mode = "normal",
                    result = levelCompletionStatus.ToString(),
                    use_hint = this.useHint.ToString().ToLower(),
                    use_skip = this.useSkip.ToString().ToLower(),
                    play_duration = (int)playDurationMs,
                    false_count = this.falseCount,
                    life_count = this.lifeSystem.CurrentLifeCount,
                    play_index = playIndex,
                    win_index = winIndex
                });
            }
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelCompletedSub.Dispose();
            this.levelStartSub.Dispose();
            this.levelHintSub.Dispose();
            this.levelSkipSub.Dispose();
            this.actionFailedSub.Dispose();
            this.ads.Interstitial.OnShowSucceeded -= OnAdOpened;
            this.ads.Interstitial.OnClosed -= OnAdClosed;
            this.ads.RewardVideo.OnVideoOpened -= OnRewardOpened;
            this.ads.RewardVideo.OnVideoClosed -= OnRewardClosed;
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