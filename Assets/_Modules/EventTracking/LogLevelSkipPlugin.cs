using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events.AsyncBus;
using Mimi.Games.Events;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

namespace Tracking
{
    public class LogLevelSkipPlugin : IPlugin
    {
        private readonly RuntimeState runtimeState;
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAnalyticTracker analyticTracker;
        private IDisposable levelSkipSub;

        public LogLevelSkipPlugin(RuntimeState runtimeState, IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker)
        {
            this.runtimeState = runtimeState;
            this.eventSubscriber = eventSubscriber;
            this.analyticTracker = analyticTracker;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelSkipSub = this.eventSubscriber.Subscribe<SkipLevel>(SkipHandler);
        }

        private async UniTask SkipHandler(SkipLevel skipLevel, CancellationToken cancellation)
        {
            await UniTask.CompletedTask;
            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
            Debug.Log($"--- (TRACKING) Log level Skip: {currentLevelOrder}");

            this.analyticTracker.LogEvent(new Feature_SKIP()
            {
                eventName = Feature_SKIP.EVENT_NAME.skip,
                level = currentLevelOrder.ToString(),
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
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