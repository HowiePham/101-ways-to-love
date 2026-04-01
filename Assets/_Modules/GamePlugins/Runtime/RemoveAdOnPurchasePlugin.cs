using System.Linq;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using Mimi.Ads.Adapters;
using Mimi.Games.Plugins;
using Mimi.IAP;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using UnityEngine;

namespace Ads
{
    public class RemoveAdOnPurchasePlugin : IPlugin
    {
        private GameContext gameContext;

        public RemoveAdOnPurchasePlugin(GameContext gameContext)
        {
            this.gameContext = gameContext;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.gameContext.InAppPurchaseStore.PurchaseCompleted += PurchaseCompletedHandler;
            DebugLogConsole.AddCommandInstance("buy-remove-ads-iap", "Buy Remove Ads", "RemoveAds", this);
        }

        private void PurchaseCompletedHandler(PurchaseReceipt receipt)
        {
            string productKey = ProductKey.RemoveAds_Android;
            if (!receipt.Order.ProductIds.Any(x => x.Contains(productKey))) return;
            RemoveAds();
        }

        private void RemoveAds()
        {
            this.gameContext.Ads.Banner.Hide();
            this.gameContext.Ads.Banner.Destroy();
            this.gameContext.Ads.Mrec.Hide();
            this.gameContext.Ads.Mrec.Destroy();
            this.gameContext.Ads.SetBanner(NullBannerAdapter.Instance);
            this.gameContext.Ads.SetInterstitial(EditorInterstitialAdapter.Instance);
            this.gameContext.Ads.SetAppOpen(NullAppOpenAdapter.Instance);
            PlayerPrefs.SetInt("RemoveAdsCheat", 1);
            Messenger.Broadcast(EventKey.RemoveAdsCompleted);
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.gameContext.InAppPurchaseStore.PurchaseCompleted -= PurchaseCompletedHandler;
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