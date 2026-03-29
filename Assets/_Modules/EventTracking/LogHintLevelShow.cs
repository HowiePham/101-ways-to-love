using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events.AsyncBus;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

namespace Tracking
{
    public class LogHintLevelShow : IPlugin
    {
        private readonly RuntimeState runtimeState;
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAnalyticTracker analyticTracker;
        private IDisposable levelHintSub;

        public LogHintLevelShow(RuntimeState runtimeState, IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker)
        {
            this.runtimeState = runtimeState;
            this.eventSubscriber = eventSubscriber;
            this.analyticTracker = analyticTracker;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelHintSub = this.eventSubscriber.Subscribe<UseHint>(HintHandler);
        }

        private async UniTask HintHandler(UseHint useHint, CancellationToken cancellation)
        {
            await UniTask.CompletedTask;
            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
            Debug.Log($"--- (TRACKING) Log level Hint: {currentLevelOrder}");

            this.analyticTracker.LogEvent(new Feature_HINT()
            {
                eventName = Feature_HINT.EVENT_NAME.hint,
                level = currentLevelOrder.ToString(),
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelHintSub.Dispose();
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