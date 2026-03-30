using System;
using Cysharp.Threading.Tasks;
using Economy.Resources;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Games.Plugins;
using UnityEngine;

namespace Tracking
{
    public class LogResourceChangedPlugin : IPlugin
    {
        private readonly ResourceCollection playerResources;
        private readonly IAnalyticTracker analyticTracker;

        public LogResourceChangedPlugin(
            ResourceCollection playerResources,
            IAnalyticTracker analyticTracker)
        {
            this.playerResources = playerResources;
            this.analyticTracker = analyticTracker;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.playerResources.ResourceChanged += OnResourceChanged;
        }

        private void OnResourceChanged(object sender, ResourceChangedEventArgs e)
        {
            string placement = e.context?.Placement ?? "unknown";

            if (e.CurrentAmount > e.PreviousAmount)
            {
                float earned = e.CurrentAmount - e.PreviousAmount;
                Debug.Log($"--- (TRACKING) Resource Earn: {e.Id} +{earned} placement={placement}");
                this.analyticTracker.LogEvent(new Feature_RESOURCE_EARN
                {
                    eventName        = Feature_RESOURCE_EARN.EVENT_NAME.resource_earn,
                    resource_type    = "currency",
                    resource_name    = e.Id,
                    resource_amount  = earned.ToString(),
                    placement        = placement,
                    resource_balance = e.CurrentAmount.ToString()
                });
            }
            else if (e.CurrentAmount < e.PreviousAmount)
            {
                float spent = e.PreviousAmount - e.CurrentAmount;
                Debug.Log($"--- (TRACKING) Resource Spend: {e.Id} -{spent} placement={placement}");
                this.analyticTracker.LogEvent(new Feature_RESOURCE_SPEND
                {
                    eventName        = Feature_RESOURCE_SPEND.EVENT_NAME.resource_spend,
                    resource_type    = "currency",
                    resource_name    = e.Id,
                    resource_amount  = spent.ToString(),
                    placement        = placement,
                    resource_balance = e.CurrentAmount.ToString()
                });
            }
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.playerResources.ResourceChanged -= OnResourceChanged;
        }

        public async UniTask Begin() { await UniTask.CompletedTask; }
        public async UniTask End()   { await UniTask.CompletedTask; }
    }
}
