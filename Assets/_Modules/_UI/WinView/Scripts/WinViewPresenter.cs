using _Modules._UI.LoseView.Scripts;
using _Modules.GameEvent.Scripts;
using Cysharp.Threading.Tasks;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Configs;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.LevelManagement;
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
        private readonly ILevelOrder levelOrder;
        private readonly IConfigProvider remoteConfig;
        private readonly LifeSystem lifeSystem;
        private readonly ChapterLevelRepository chapterLevelRepo;

        public WinViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher,
            RuntimeState runtimeState, IAdAdapter adsAdapter,
            LevelConfig showAdLevelConfig, GameData gameData, ILevelOrder levelOrder, IConfigProvider remoteConfig, LifeSystem lifeSystem,
            ChapterLevelRepository chapterLevelRepo) : base(scenePresenter, transform)
        {
            this.eventPublisher = eventPublisher;
            this.runtimeState = runtimeState;
            this.adsAdapter = adsAdapter;
            this.showAdLevelConfig = showAdLevelConfig;
            this.gameData = gameData;
            this.remoteConfig = remoteConfig;
            this.lifeSystem = lifeSystem;
            this.levelOrder = levelOrder;
            this.chapterLevelRepo = chapterLevelRepo;
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

            Messenger.AddListener(EventKey.RemoveAdsCompleted, HideRemoveAdsButton);

            var baseGameContext = (BaseGameContext)this.Context;
            this.winView.SetActiveRemoveAdsButton(!baseGameContext.IsRemoveAds);

            this.adsAdapter.Mrec.Show(new AdPlacement("win_view"));
            bool isShowNextChapterInWinView = this.remoteConfig.GetValue(ConfigKey.ShowNextChapterInWinView).Boolean;

            if (isShowNextChapterInWinView && CanShowNextChapter())
            {
                int lifeReward = this.remoteConfig.GetValue(ConfigKey.LifeRecoverAfterChapter).Int;
                this.lifeSystem.AddLives(lifeReward, "win_view");

                int currentOrder = this.runtimeState.CurrentLevelOrder.Value;
                LevelInfo nextLevel = this.levelOrder.GetNextLevel(currentOrder);

                if (nextLevel != null)
                {
                    ChapterInfo nextChapter = this.chapterLevelRepo.GetChapter(nextLevel.Chapter);
                    Sprite icon = Resources.Load<Sprite>("Icons/" + nextChapter.ChapterIconAddress);
                    this.winView.SetNextChapterHint(true, icon);
                }
            }
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
            Messenger.RemoveListener(EventKey.RemoveAdsCompleted, HideRemoveAdsButton);

            this.adsAdapter.Mrec.Hide();
            this.winView.SetNextChapterHint(false);
            // this.currencyView.OnAddCurrencyClicked -= AddCurrencyClickedHandler;
        }

        private void HideRemoveAdsButton()
        {
            this.winView.SetActiveRemoveAdsButton(false);
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
            bool isAdCooldownCompletedAfterRewarded = gameContext.GameData.IsAdCoolDownCompletedAfterReward;
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
            bool showAds = allowShowAd && isAdAvailable && !adCooldown && isAdCooldownCompletedAfterRewarded;
            Debug.LogError("--- (NEXT) Ad Available Interstitial: " + isAdAvailable);
            Debug.LogError("--- (NEXT) Ad Level: " + allowShowAd);
            Debug.LogError("--- (NEXT) ads cooldown: " + adCooldown);
            Debug.LogError("--- (NEXT) ads cooldown completed after rewarded: " + isAdCooldownCompletedAfterRewarded);
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

        private bool CanShowNextChapter()
        {
            return this.runtimeState.IsNewChapterUnlocked.Value;
        }

        private void NextLevelHandler()
        {
            if (CanShowNextChapter())
            {
                ShowChapterUnlockThenReward().Forget();
                return;
            }

            this.eventPublisher.PublishAsync(new NextLevelClicked());
            Hide();
        }

        private async UniTaskVoid ShowChapterUnlockThenReward()
        {
            Hide();

            var chapterUnlockPresenter = this.ScenePresenter.GetViewPresenter<ChapterUnlockPresenter>();
            bool chapterShown = await chapterUnlockPresenter.TryShowForNextChapterAndWait();

            var lifeRewardPresenter = this.ScenePresenter.GetViewPresenter<RewardPresenter>();
            await lifeRewardPresenter.ShowAndWait();

            if (chapterShown && chapterUnlockPresenter.WasBackHomeRequested)
                this.eventPublisher.PublishAsync(new BackHome());
            else
                this.eventPublisher.PublishAsync(new NextLevelClicked());
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
            settingViewPresenter.SetActiveHomeButton(false);
            settingViewPresenter.SetActiveReplayButton(false);
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