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

            int playIndex = PlayerPrefs.GetInt($"play_index_{currentLevelOrder}", 0) + 1;
            PlayerPrefs.SetInt($"play_index_{currentLevelOrder}", playIndex);

            Debug.Log($"--- (TRACKING) Log level Start: {currentLevelOrder} --- PlayIndex: {playIndex}");

            UpdateUserProperties(currentLevelOrder);

            this.analyticTracker.LogEvent(new LevelStartEventData()
            {
                eventName = LevelStartEventData.EVENT_NAME.level_start,
                level = currentLevelOrder.ToString(),
                level_mode = "normal",
                life_count = this.lifeSystem.CurrentLifeCount,
                play_index = playIndex
            });
        }

        private void UpdateUserProperties(int currentLevelOrder)
        {
            IUserPropertyData userProperty = new USER_PROPERTIES()
            {
                user_properties = USER_PROPERTIES_TYPE.current_level,
                value = currentLevelOrder.ToString()
            };
            this.analyticTracker.SetUserProperties(userProperty);
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