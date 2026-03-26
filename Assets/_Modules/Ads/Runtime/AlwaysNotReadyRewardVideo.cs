using System;
using Mimi.Ads.Adapters;

namespace _Modules.Ads
{
    public class AlwaysNotReadyRewardVideo : IRewardVideoAdapter
    {
        public static readonly AlwaysNotReadyRewardVideo Instance = new AlwaysNotReadyRewardVideo();

        public void Load()
        {
        }

        public void Show(AdReward adReward, AdPlacement placement)
        {
        }

        public bool IsReady => false;
        public event Action OnLoadSucceeded;
        public event Action<AdError> OnLoadFailed;
        public event Action<AdReward, AdError> OnShowFailed;
        public event Action<AdReward> OnVideoOpened;
        public event Action<AdReward> OnVideoClosed;
        public event Action<AdReward> OnVideoClicked;
        public event Action<AdReward> OnRewarded;
        public event Action<ImpressionData> OnImpressionSuccess;
    }
}