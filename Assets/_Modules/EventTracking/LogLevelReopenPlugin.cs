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
    public class LogLevelReopenPlugin : IPlugin
    {
        private readonly RuntimeState runtimeState;
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAnalyticTracker analyticTracker;

        private IDisposable levelStartedSub;
        private IDisposable levelCompletedSub;
        private IDisposable gamePausedSub;
        private IDisposable gameUnpausedSub;

        private bool isInLevel;
        private bool hasExited;

        public LogLevelReopenPlugin(RuntimeState runtimeState, IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker)
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
            this.gameUnpausedSub = this.eventSubscriber.Subscribe<GameUnpaused>(GameUnpausedHandler);
        }

        private async UniTask LevelStartedHandler(LevelStarted levelStarted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.isInLevel = true;
            this.hasExited = false;
        }

        private async UniTask LevelCompletedHandler(LevelCompleted levelCompleted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.isInLevel = false;
            this.hasExited = false;
        }

        private async UniTask GamePausedHandler(GamePaused gamePaused, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            if (this.isInLevel)
            {
                this.hasExited = true;
            }
        }

        private async UniTask GameUnpausedHandler(GameUnpaused gameUnpaused, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            if (!this.isInLevel || !this.hasExited) return;

            this.hasExited = false;
            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;

            Debug.Log($"--- (TRACKING) Level Reopen: {currentLevelOrder}");
            this.analyticTracker.LogEvent(new Feature_LEVEL_REOPEN
            {
                eventName = Feature_LEVEL_REOPEN.EVENT_NAME.level_reopen,
                level = currentLevelOrder.ToString(),
                mode = "normal"
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelStartedSub.Dispose();
            this.levelCompletedSub.Dispose();
            this.gamePausedSub.Dispose();
            this.gameUnpausedSub.Dispose();
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
