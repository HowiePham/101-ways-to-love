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
    public class LogLevelStartPlugin : IPlugin
    {
        private readonly RuntimeState runtimeState;
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAnalyticTracker analyticTracker;
        private readonly LifeSystem lifeSystem;
        private IDisposable levelStartedSub;

        public LogLevelStartPlugin(RuntimeState runtimeState, IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker, LifeSystem lifeSystem)
        {
            this.runtimeState = runtimeState;
            this.eventSubscriber = eventSubscriber;
            this.analyticTracker = analyticTracker;
            this.lifeSystem = lifeSystem;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelStartedSub = this.eventSubscriber.Subscribe<LevelStarted>(LevelStartedHandler);
        }

        private async UniTask LevelStartedHandler(LevelStarted levelStarted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
            Debug.Log($"--- (TRACKING) Log level Start: {currentLevelOrder}");

            this.analyticTracker.LogEvent(new Feature_LEVEL_START()
            {
                eventName = Feature_LEVEL_START.EVENT_NAME.level_start,
                level = currentLevelOrder.ToString(),
                level_mode = "normal",
                life_count = this.lifeSystem.CurrentLifeCount.ToString()
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelStartedSub.Dispose();
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