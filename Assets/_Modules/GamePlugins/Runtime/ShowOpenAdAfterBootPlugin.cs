using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Configs;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Games.Plugins;
using UnityEngine;

namespace Ads
{
    public class ShowOpenAdAfterBootPlugin : IPlugin
    {
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAdAdapter adAdapter;
        private readonly IConfigProvider remoteConfig;
        private bool isFirstSession;

        private IDisposable bootGameCompletedEventHandler;

        public ShowOpenAdAfterBootPlugin(IAdAdapter adAdapter, IAsyncSubscriber eventSubscriber, IConfigProvider remoteConfig, bool isFirstSession)
        {
            this.adAdapter = adAdapter;
            this.eventSubscriber = eventSubscriber;
            this.remoteConfig = remoteConfig;
            this.isFirstSession = isFirstSession;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.bootGameCompletedEventHandler =
                this.eventSubscriber.Subscribe<BootGameCompleted>(BootGameCompletedHandler);
        }

        private async UniTask BootGameCompletedHandler(BootGameCompleted bootGameCompleted,
            CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            if (this.remoteConfig.GetValue(ConfigKey.IsShowAOA).Boolean)
            {
                Debug.Log($"--- (PLUGIN) Showing Open Ad After boot game handling...");

                if (this.isFirstSession)
                {
                    if (this.remoteConfig.GetValue(ConfigKey.ShowAOAFirstOpen).Boolean)
                    {
                        Debug.Log("[AOA] Call Show AOA First Session");
                        this.adAdapter.AppOpen.Show(new AdPlacement("start_game"));
                    }
                }
                else
                {
                    Debug.Log("[AOA] Call Show AOA after booting");
                    this.adAdapter.AppOpen.Show(new AdPlacement("start_game"));
                }
            }
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.bootGameCompletedEventHandler.Dispose();
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