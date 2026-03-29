using Mimi.Ads.Adapters;
using Singular;

namespace Games
{
    public class SingularLogInterstitial : InterstitialDecorator
    {
        public SingularLogInterstitial(IInterstitialAdapter adapter) : base(adapter)
        {
        }

        public override void Show(AdPlacement placement)
        {
            base.Show(placement);
            SingularSDK.Event("sgn_inters_ad_eligible");
        }

        protected override void LoadSucceededHandler()
        {
            base.LoadSucceededHandler();
            SingularSDK.Event("sgn_inters_api_called");
        }

        protected override void ShowSucceededHandler(AdPlacement placement)
        {
            base.ShowSucceededHandler(placement);
            SingularSDK.Event("sgn_inters_displayed");
        }
    }
}