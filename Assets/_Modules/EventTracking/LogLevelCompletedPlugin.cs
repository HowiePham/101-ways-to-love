using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events;
using Mimi.Games.Events;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

namespace Tracking
{
    public class LogLevelCompletedPlugin : MonoPlugin
    {
        [SerializeField] private GameContext gameContext;

        private IDisposable levelCompletedSub;
        private IDisposable levelStartSub;
        private IDisposable levelSkipSub;
        private IDisposable levelHintSub;
        private bool useHelp;
        private DateTime startTime;

        public override async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.levelCompletedSub = this.gameContext.EventSubscriber.Subscribe<LevelCompleted>(LevelCompletedHandler);
            this.levelStartSub = this.gameContext.EventSubscriber.Subscribe<LevelStarted>(LevelStartedHandler);
            this.levelSkipSub = this.gameContext.EventSubscriber.Subscribe<SkipLevel>(SkipHandler);
            this.levelHintSub = this.gameContext.EventSubscriber.Subscribe<UseHint>(HintHandler);
        }

        private async UniTask SkipHandler(SkipLevel skipLevel, CancellationToken cancellation)
        {
            await UniTask.CompletedTask;
            this.useHelp = true;
        }

        private async UniTask HintHandler(UseHint useHint, CancellationToken cancellation)
        {
            await UniTask.CompletedTask;
            this.useHelp = true;
        }

        private async UniTask LevelStartedHandler(LevelStarted levelStarted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            this.useHelp = false;
            this.startTime = DateTime.UtcNow;
        }

        private async UniTask LevelCompletedHandler(LevelCompleted levelCompleted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            int currentLevelOrder = this.gameContext.RuntimeState.CurrentLevelOrder.Value + 1;

            this.gameContext.AnalyticTracker.LogEvent(new Feature_LEVEL_END()
            {
                eventName = Feature_LEVEL_END.EVENT_NAME.level_end,
                level = currentLevelOrder.ToString(),
                level_mode = "normal",
                success = "true"
            });
        }

        public override async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.levelCompletedSub.Dispose();
            this.levelStartSub.Dispose();
            this.levelHintSub.Dispose();
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