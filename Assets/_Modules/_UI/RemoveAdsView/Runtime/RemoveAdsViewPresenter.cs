using Mimi.Prototypes.UI;
using UnityEngine;

public class RemoveAdsViewPresenter : BaseViewPresenter
{
    private RemoveAdsView removeAdsView;

    public RemoveAdsViewPresenter(BaseScenePresenter scenePresenter, Transform transform) : base(scenePresenter, transform)
    {
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
    }

    private void BuyRemoveAdsHandler()
    {
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