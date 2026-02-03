using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

namespace Tracking
{
    public class LogLevelStartPlugin : MonoPlugin
    {
        [SerializeField] private GameContext gameContext;

        private IDisposable levelStartedSub;

        public override async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelStartedSub = this.gameContext.EventSubscriber.Subscribe<LevelStarted>(LevelStartedHandler);
        }

        private async UniTask LevelStartedHandler(LevelStarted levelStarted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            int currentLevelOrder = this.gameContext.RuntimeState.CurrentLevelOrder.Value + 1;
            this.gameContext.AnalyticTracker.LogEvent(new Feature_LEVEL_START()
            {
                eventName = Feature_LEVEL_START.EVENT_NAME.level_start,
                level = currentLevelOrder.ToString(),
                level_mode = "normal"
            });
        }

        public override async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelStartedSub.Dispose();
        }

        public override async UniTask Begin()
        {
            await UniTask.CompletedTask;
        }

        public override async UniTask End()
        {
            await UniTask.CompletedTask;
        }
    }
}