using Mimi.Ads.Adapters;
using Singular;

namespace Games
{
    public class SingularLogRewardVideo : RewardVideoDecorator
    {
        public SingularLogRewardVideo(IRewardVideoAdapter adapter) : base(adapter)
        {
        }

        public override void Show(AdReward reward, AdPlacement placement)
        {
            base.Show(reward, placement);
            SingularSDK.Event("sgn_rewarded_ad_eligible");
        }

        protected override void LoadSucceededHandler()
        {
            base.LoadSucceededHandler();
            SingularSDK.Event("sgn_rewarded_api_called");
        }

        protected override void VideoOpenedHandler(AdReward adReward)
        {
            base.VideoOpenedHandler(adReward);
            SingularSDK.Event("sgn_rewarded_displayed");
        }
    }
}