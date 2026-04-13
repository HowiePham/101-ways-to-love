using _Modules.GameEvent.Scripts;
using Mimi.Events.AsyncBus;
using Mimi.IAP;
using Mimi.Prototypes;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.Purchasing;

public class RemoveAdsViewPresenter : BaseViewPresenter
{
    private RemoveAdsView removeAdsView;
    private DialogManager dialogManager;
    private IPurchasingProvider purchasingProvider;
    private readonly IAsyncPublisher eventPublisher;

    public RemoveAdsViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, DialogManager dialogManager, IPurchasingProvider purchasingProvider) :
        base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.dialogManager = dialogManager;
        this.purchasingProvider = purchasingProvider;
    }

    protected override void AddViews()
    {
        this.removeAdsView = AddView<RemoveAdsView>();
    }

    protected override void AddChildren()
    {
    }

    protected override void OnShow()
    {
        base.OnShow();

        this.removeAdsView.OnBuyRemoveAdsClicked += BuyRemoveAdsHandler;
        this.removeAdsView.OnCloseClicked += CloseClickedHandler;
        this.purchasingProvider.PurchaseFailed += PurchasFailedHandler;
        this.purchasingProvider.PurchaseCompleted += PurchaseCompletedHandler;
        this.eventPublisher.PublishAsync(new IapShow("remove_ads_view", "pack", "click", "remove_ads"));

        SetRemoveAdsPriceText();
    }

    private void PurchaseCompletedHandler(PurchaseReceipt receipt)
    {
        Hide();
    }

    private void SetRemoveAdsPriceText()
    {
        var gameContext = (BaseGameContext)this.Context;
        string removeAdsProductId = gameContext.RemoveAdsProductId;
        foreach (IProduct product in this.purchasingProvider.Products)
        {
            if (!product.Id.Equals(removeAdsProductId))
            {
                continue;
            }

            this.removeAdsView.SetPriceText(product.LocalizedPriceWithCurrencyCode);
        }
    }

    private void PurchasFailedHandler(PurchaseError error)
    {
        if (this.dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide, out AutoHideNotificationDialog dialog))
        {
            dialog.SetText("Purchase Failed");
        }
    }

    private void BuyRemoveAdsHandler()
    {
        var gameContext = (BaseGameContext)this.Context;
        string removeAdsProductId = gameContext.RemoveAdsProductId;

        this.eventPublisher.PublishAsync(new IapClick("remove_ads_view", "pack", "click", "remove_ads"));
        this.purchasingProvider.Purchase(removeAdsProductId, new PurchaseContext("remove_ads_view"));
    }

    private void CloseClickedHandler()
    {
        Hide();
    }

    protected override void OnHide()
    {
        base.OnHide();

        this.removeAdsView.OnBuyRemoveAdsClicked -= BuyRemoveAdsHandler;
        this.removeAdsView.OnCloseClicked -= CloseClickedHandler;
        this.purchasingProvider.PurchaseFailed -= PurchasFailedHandler;
        this.purchasingProvider.PurchaseCompleted -= PurchaseCompletedHandler;
    }
}