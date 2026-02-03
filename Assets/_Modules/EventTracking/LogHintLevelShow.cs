using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

namespace Tracking
{
    public class LogHintLevelShow : MonoPlugin
    {
        [SerializeField] private GameContext gameContext;
        private IDisposable levelHintSub;

        public override async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelHintSub = this.gameContext.EventSubscriber.Subscribe<UseHint>(HintHandler);
        }

        private async UniTask HintHandler(UseHint useHint, CancellationToken cancellation)
        {
            await UniTask.CompletedTask;
            int currentLevelOrder = this.gameContext.RuntimeState.CurrentLevelOrder.Value + 1;

            this.gameContext.AnalyticTracker.LogEvent(new Feature_HINT()
            {
                eventName = Feature_HINT.EVENT_NAME.hint,
                level = currentLevelOrder.ToString(),
            });
        }

        public override async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelHintSub.Dispose();
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