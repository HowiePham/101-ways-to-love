using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        private IDisposable levelStartedSub;
        private IDisposable levelCompletedSub;
        private IDisposable gamePausedSub;

        private bool isInLevel;
        private DateTime levelStartTime;

        public LogLevelExitPlugin(RuntimeState runtimeState, IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker)
        {
            this.runtimeState = runtimeState;
            this.eventSubscriber = eventSubscriber;
            this.analyticTracker = analyticTracker;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelStartedSub = this.eventSubscriber.Subscribe<LevelStarted>(LevelStartedHandler);
            this.levelCompletedSub = this.eventSubscriber.Subscribe<LevelCompleted>(LevelCompletedHandler);
            this.gamePausedSub = this.eventSubscriber.Subscribe<GamePaused>(GamePausedHandler);
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

        private async UniTask GamePausedHandler(GamePaused gamePaused, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            if (!this.isInLevel) return;

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
