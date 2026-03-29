using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Games.Events;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

namespace Tracking
{
    public class LogLevelCompletedPlugin : IPlugin
    {
        private GameContext gameContext;
        private readonly RuntimeState runtimeState;
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAnalyticTracker analyticTracker;

        private IDisposable levelCompletedSub;
        private IDisposable levelStartSub;
        private IDisposable levelSkipSub;
        private IDisposable levelHintSub;
        private bool useSkip;
        private bool useHint;
        private DateTime startTime;

        public LogLevelCompletedPlugin(RuntimeState runtimeState, IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker)
        {
            this.runtimeState = runtimeState;
            this.eventSubscriber = eventSubscriber;
            this.analyticTracker = analyticTracker;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelCompletedSub = this.eventSubscriber.Subscribe<LevelCompleted>(LevelCompletedHandler);
            this.levelStartSub = this.eventSubscriber.Subscribe<LevelStarted>(LevelStartedHandler);
            this.levelSkipSub = this.eventSubscriber.Subscribe<SkipLevel>(SkipHandler);
            this.levelHintSub = this.eventSubscriber.Subscribe<UseHint>(HintHandler);
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

        private async UniTask LevelStartedHandler(LevelStarted levelStarted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.useHint = false;
            this.useSkip = false;
            this.startTime = DateTime.UtcNow;
        }

        private async UniTask LevelCompletedHandler(LevelCompleted levelCompleted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
            Debug.Log($"--- (TRACKING) Log level Completed: {currentLevelOrder} --- Hint: {this.useHint} --- Skip: {this.useSkip}");

            this.analyticTracker.LogEvent(new Feature_LEVEL_END()
            {
                eventName = Feature_LEVEL_END.EVENT_NAME.level_end,
                level = currentLevelOrder.ToString(),
                level_mode = "normal",
                success = "true",
                use_hint = this.useHint.ToString().ToLower(),
                use_skip = this.useSkip.ToString().ToLower()
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelCompletedSub.Dispose();
            this.levelStartSub.Dispose();
            this.levelHintSub.Dispose();
            this.levelSkipSub.Dispose();
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