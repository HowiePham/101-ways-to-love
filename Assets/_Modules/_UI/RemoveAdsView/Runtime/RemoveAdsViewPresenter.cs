using _Modules.GameEvent.Scripts;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes.UI;
using UnityEngine;

public class RemoveAdsViewPresenter : BaseViewPresenter
{
    private RemoveAdsView removeAdsView;
    private readonly IAsyncPublisher eventPublisher;
    private readonly string placement;

    public RemoveAdsViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, string placement = "ingame_icon") : base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.placement = placement;
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

        this.eventPublisher.PublishAsync(new IapShow(this.placement, "pack", "click", "removeads"));
    }

    private void BuyRemoveAdsHandler()
    {
        this.eventPublisher.PublishAsync(new IapClick(this.placement, "pack", "click", "removeads"));
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
    }
}