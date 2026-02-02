using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Games.Events;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

namespace Tracking
{
    public class LogLevelSkipPlugin : MonoPlugin
    {
        [SerializeField] private GameContext gameContext;
        private IDisposable levelSkipSub;

        public override async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelSkipSub = this.gameContext.EventSubscriber.Subscribe<SkipLevel>(SkipHandler);
        }

        private async UniTask SkipHandler(SkipLevel skipLevel, CancellationToken cancellation)
        {
            await UniTask.CompletedTask;
            int currentLevelOrder = this.gameContext.RuntimeState.CurrentLevelOrder.Value + 1;

            this.gameContext.AnalyticTracker.LogEvent(new Feature_SKIP()
            {
                eventName = Feature_SKIP.EVENT_NAME.skip,
                level = currentLevelOrder.ToString(),
            });
        }

        public override async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelSkipSub.Dispose();
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