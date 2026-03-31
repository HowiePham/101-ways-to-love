using _Modules._UI.LoseView.Scripts;
using _Modules.GameEvent.Scripts;
using Mimi.Ads.Adapters;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes;
using Mimi.Prototypes.UI;
using Mimi.Rx.Variables;
using UnityEngine;

namespace _Modules._UI.WinView.Scripts
{
    public class WinViewPresenter : BaseViewPresenter
    {
        private WinView winView;
        private CurrencyView currencyView;
        private readonly IAsyncPublisher eventPublisher;
        private readonly LevelConfig showAdLevelConfig;
        private readonly IAdAdapter adsAdapter;
        private readonly RuntimeState runtimeState;
        private readonly GameData gameData;

        public WinViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher,
            RuntimeState runtimeState, IAdAdapter adsAdapter,
            LevelConfig showAdLevelConfig, GameData gameData) : base(scenePresenter,
            transform)
        {
            this.eventPublisher = eventPublisher;
            this.runtimeState = runtimeState;
            this.adsAdapter = adsAdapter;
            this.showAdLevelConfig = showAdLevelConfig;
            this.gameData = gameData;
        }

        protected override void AddViews()
        {
            this.winView = AddView<WinView>();
            // this.currencyView = AddView<CurrencyView>();
        }

        protected override void AddChildren()
        {
        }

        protected override void OnShow()
        {
            base.OnShow();

            this.winView.OnContinueClicked += ContinueClickedHandler;
            this.winView.OnReplayClicked += ReplayClickedHandler;
            this.winView.OnRemoveAdsClicked += ShowRemoveAdsView;
            this.winView.OnSettingClicked += SettingClickedHandler;
            this.winView.OnHomeClicked += HomeClickedHandler;
            this.adsAdapter.Interstitial.OnShowFailed += InterstitialShowFailedHandler;
            this.adsAdapter.Interstitial.OnClosed += InterstitialClosedHandler;
            
            this.adsAdapter.Mrec.Show(new AdPlacement("win_view"));
            // this.currencyView.OnAddCurrencyClicked += AddCurrencyClickedHandler;
        }

        protected override void OnHide()
        {
            base.OnHide();

            this.winView.OnContinueClicked -= ContinueClickedHandler;
            this.winView.OnReplayClicked -= ReplayClickedHandler;
            this.winView.OnRemoveAdsClicked -= ShowRemoveAdsView;
            this.winView.OnSettingClicked -= SettingClickedHandler;
            this.winView.OnHomeClicked -= HomeClickedHandler;
            this.adsAdapter.Interstitial.OnShowFailed -= InterstitialShowFailedHandler;
            this.adsAdapter.Interstitial.OnClosed -= InterstitialClosedHandler;
            
            this.adsAdapter.Mrec.Hide();
            // this.currencyView.OnAddCurrencyClicked -= AddCurrencyClickedHandler;
        }

        private void ShowRemoveAdsView()
        {
            var removeAdsPresenter = this.ScenePresenter.GetViewPresenter<RemoveAdsViewPresenter>();
            removeAdsPresenter.Show();
        }

        private void AddCurrencyClickedHandler()
        {
            Debug.Log($"--- (Currency) Add currency clicked");
        }

        private void ContinueClickedHandler()
        {
            var gameContext = Context as GameContext;
            bool isAdAvailable = !gameContext.IsRemoveAds && this.adsAdapter.Interstitial.IsReady;
            bool allowShowAd = false;

            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
            if (this.showAdLevelConfig.HasLevel(currentLevelOrder.ToString()))
            {
                allowShowAd = true;
            }
            else
            {
                if (currentLevelOrder > this.showAdLevelConfig.MaxLevel)
                {
                    allowShowAd = true;
                }
            }

            bool adCooldown = this.gameData.IsAdCoolDowning;
            bool showAds = allowShowAd && isAdAvailable && !adCooldown;
            Debug.LogError("--- (NEXT) Ad Available Interstitial: " + isAdAvailable);
            Debug.LogError("--- (NEXT) Ad Level: " + allowShowAd);
            Debug.LogError("--- (NEXT) ads cooldown: " + adCooldown);
            Debug.LogError("--- (NEXT) showAds: " + showAds);

            if (showAds)
            {
                this.adsAdapter.Interstitial.Show(new AdPlacement("level_complete"));
            }
            else
            {
                NextLevelHandler();
            }
        }

        private void NextLevelHandler()
        {
            var chapterUnlockPresenter = this.ScenePresenter.GetViewPresenter<ChapterUnlockPresenter>();
            if (chapterUnlockPresenter.TryShowForNextChapter())
            {
                Hide();
                return;
            }

            this.eventPublisher.PublishAsync(new NextLevelClicked());
            Hide();
        }

        private void ReplayClickedHandler()
        {
            this.eventPublisher.PublishAsync(new LevelTryAgain());
            Hide();
        }

        private void SettingClickedHandler()
        {
            var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
            settingViewPresenter.Show();
        }

        private void HomeClickedHandler()
        {
            this.eventPublisher.PublishAsync(new BackHome());
            Hide();
        }

        private void InterstitialClosedHandler(AdPlacement adPlacement)
        {
            if (adPlacement.location == "level_complete")
            {
                NextLevelHandler();
            }
        }

        private void InterstitialShowFailedHandler(AdError adError)
        {
            var adPlacement = adError.Placement;
            if (adPlacement.location == "level_complete")
            {
                NextLevelHandler();
            }
        }
    }
}