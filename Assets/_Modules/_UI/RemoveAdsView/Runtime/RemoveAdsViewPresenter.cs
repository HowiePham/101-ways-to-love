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
        this.eventPublisher.PublishAsync(new IapShow("remove_ads_view", "pack", "click", "remove_ads"));
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
        this.eventPublisher.PublishAsync(new IapClick("remove_ads_view", "pack", "click", "remove_ads"));
        this.purchasingProvider.Purchase(ProductKey.RemoveAds_Android, new PurchaseContext("remove_ads_view"));
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
    }
}