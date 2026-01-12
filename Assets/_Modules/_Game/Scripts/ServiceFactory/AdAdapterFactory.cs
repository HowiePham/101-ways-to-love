using Cysharp.Threading.Tasks;
using Games;
using Mimi.Ads.Adapters;
using UnityEngine;

namespace Mimi.Prototypes
{
    public class AdAdapterFactory : IServiceFactory<IAdAdapter>
    {
        public async UniTask<IAdAdapter> CreateService()
        {
            await UniTask.CompletedTask;
            IAdAdapter adAdapter;

            if (Application.isEditor)
            {
                adAdapter = DebugAdAdapter.Instance;
                adAdapter.SetInterstitial(EditorInterstitialAdapter.Instance);
                adAdapter.SetRewardVideo(EditorRewardVideoAdapter.Instance);
                return adAdapter;
            }

            SetupAdConsents();
            adAdapter = CreateAdAdapter();
            return adAdapter;
        }

        private void SetupAdConsents()
        {
#if APPLOVIN_MAX
            MaxSdk.SetHasUserConsent(true);
            MaxSdk.SetIsAgeRestrictedUser(false);
            MaxSdk.SetDoNotSell(false);
#elif IRONSOURCE
            IronSource.Agent.setConsent(true);
            IronSource.Agent.setMetaData("do_not_sell","false");
            IronSource.Agent.setMetaData("is_child_directed","false");
#endif
        }

        private IAdAdapter CreateAdAdapter()
        {
            IAdAdapter adAdapter = null;
#if IRONSOURCE
            adAdapter = new Mimi.Ads.Adapters.IronSources.IronSourceAdapter(this.projectConfig.IronSourceAppKey);
#elif APPLOVIN_MAX
            adAdapter =
 new Mimi.Ads.Adapters.Max.MaxAdapter(this.projectConfig.MaxSdkKey, SystemInfo.deviceUniqueIdentifier);
#endif

#if AMAZON_AD_NETWORK && IRONSOURCE
            adAdapter =
 new Mimi.Ads.Adapters.Extensions.Amazons.IronSources.AmazonIronSourceAdapter(this.projectConfig.AmazonAppId, adAdapter);
#endif

#if AMAZON_AD_NETWORK && APPLOVIN_MAX
            adAdapter =
 new Mimi.Ads.Adapters.Extensions.Amazons.Maxs.AmazonMaxAdapter(this.projectConfig.AmazonAppId, adAdapter);
#endif
            return adAdapter;
        }
    }
}