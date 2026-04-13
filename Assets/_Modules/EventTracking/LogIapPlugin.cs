using System;
using System.Threading;
using _Modules.GameEvent.Scripts;
using Cysharp.Threading.Tasks;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Events.AsyncBus;
using Mimi.Games.Plugins;
using Mimi.IAP;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Tracking
{
    public class LogIapPlugin : IPlugin
    {
        private readonly IAsyncSubscriber eventSubscriber;
        private readonly IAnalyticTracker analyticTracker;
        private readonly IPurchasingProvider purchasingProvider;

        private IDisposable iapShowSub;
        private IDisposable iapClickSub;
        private IDisposable iapPurchaseSub;

        public LogIapPlugin(IAsyncSubscriber eventSubscriber, IAnalyticTracker analyticTracker, IPurchasingProvider purchasingProvider)
        {
            this.eventSubscriber = eventSubscriber;
            this.analyticTracker = analyticTracker;
            this.purchasingProvider = purchasingProvider;
        }

        public async UniTask Install()
        {
            await UniTask.CompletedTask;
            this.iapShowSub = this.eventSubscriber.Subscribe<IapShow>(IapShowHandler);
            this.iapClickSub = this.eventSubscriber.Subscribe<IapClick>(IapClickHandler);
            this.iapPurchaseSub = this.eventSubscriber.Subscribe<IapPurchase>(IapPurchaseHandler);
            this.purchasingProvider.PurchaseCompleted += PurchaseCompletedHandler;
        }

        private void PurchaseCompletedHandler(PurchaseReceipt receipt)
        {
            foreach (var productId in receipt.Order.ProductIds)
            {
                string placement = receipt.Context.GameLocation;
                bool tryGetProduct = this.purchasingProvider.TryGetProduct(productId, out IProduct product);

                if (!tryGetProduct)
                {
                    continue;
                }

                string price = product.LocalizedPriceWithCurrencyCode;
                string currency = product.CurrencyCode;
                string showType = "pack";
                string triggerType = "click";

                Debug.Log($"--- (TRACKING) IAP Purchase: placement={placement}, pack={productId}, price={price} {currency}");
                this.analyticTracker.LogEvent(new Feature_IAP_PURCHASE
                {
                    eventName = Feature_IAP_PURCHASE.EVENT_NAME.iap_purchase,
                    placement = placement,
                    show_type = showType,
                    trigger_type = triggerType,
                    pack_name = productId,
                    price = price,
                    currency = currency
                });
            }
        }

        private async UniTask IapShowHandler(IapShow iapShow, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            Debug.Log($"--- (TRACKING) IAP Show: placement={iapShow.Placement}, pack={iapShow.PackName}");
            this.analyticTracker.LogEvent(new Feature_IAP_SHOW
            {
                eventName = Feature_IAP_SHOW.EVENT_NAME.iap_show,
                placement = iapShow.Placement,
                show_type = iapShow.ShowType,
                trigger_type = iapShow.TriggerType,
                pack_name = iapShow.PackName
            });
        }

        private async UniTask IapClickHandler(IapClick iapClick, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            Debug.Log($"--- (TRACKING) IAP Click: placement={iapClick.Placement}, pack={iapClick.PackName}");
            this.analyticTracker.LogEvent(new Feature_IAP_CLICK
            {
                eventName = Feature_IAP_CLICK.EVENT_NAME.iap_click,
                placement = iapClick.Placement,
                show_type = iapClick.ShowType,
                trigger_type = iapClick.TriggerType,
                pack_name = iapClick.PackName
            });
        }

        private async UniTask IapPurchaseHandler(IapPurchase iapPurchase, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            Debug.Log($"--- (TRACKING) IAP Purchase: placement={iapPurchase.Placement}, pack={iapPurchase.PackName}, price={iapPurchase.Price} {iapPurchase.Currency}");
            this.analyticTracker.LogEvent(new Feature_IAP_PURCHASE
            {
                eventName = Feature_IAP_PURCHASE.EVENT_NAME.iap_purchase,
                placement = iapPurchase.Placement,
                show_type = iapPurchase.ShowType,
                trigger_type = iapPurchase.TriggerType,
                pack_name = iapPurchase.PackName,
                price = iapPurchase.Price,
                currency = iapPurchase.Currency
            });
        }

        public async UniTask Uninstall()
        {
            await UniTask.CompletedTask;
            this.iapShowSub.Dispose();
            this.iapClickSub.Dispose();
            this.iapPurchaseSub.Dispose();
            this.purchasingProvider.PurchaseCompleted -= PurchaseCompletedHandler;
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