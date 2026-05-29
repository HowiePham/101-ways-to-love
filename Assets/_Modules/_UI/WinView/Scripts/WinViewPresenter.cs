using _Modules._UI.LoseView.Scripts;
using _Modules.GameEvent.Scripts;
using Cysharp.Threading.Tasks;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Configs;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes;
using Mimi.Prototypes.Currencies;
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
        private readonly IAnalyticTracker analyticTracker;
        private readonly DialogManager dialogManager;

        public WinViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher,
            RuntimeState runtimeState, IAdAdapter adsAdapter,
            LevelConfig showAdLevelConfig, GameData gameData, ILevelOrder levelOrder, IConfigProvider remoteConfig, LifeSystem lifeSystem,
            ChapterLevelRepository chapterLevelRepo, IAnalyticTracker analyticTracker, DialogManager dialogManager) : base(scenePresenter, transform)
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
            this.analyticTracker = analyticTracker;
            this.dialogManager = dialogManager;
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
            this.eventPublisher.PublishAsync(new ScreenShown("win_view"));

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

            if (CanShowNextChapter())
            {
                int currentLife = this.lifeSystem.CurrentLifeCount;
                this.winView.SetChapterRewardData(true, currentLife);

                int lifeReward = this.remoteConfig.GetValue(ConfigKey.LifeRecoverAfterChapter).Int;
                this.lifeSystem.AddLives(lifeReward, "win_view", "unlock_chapter");

                this.winView.OnBonusClicked += BonusClickedHandler;
                this.winView.OnLoseBonusClicked += LoseBonusClickedHandler;
                this.adsAdapter.RewardVideo.OnRewarded += BonusRewardedHandler;
                this.adsAdapter.RewardVideo.OnShowFailed += BonusShowFailedHandler;
            }
            else
            {
                this.adsAdapter.Mrec.Show(new AdPlacement("win_view"));
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
            this.winView.OnBonusClicked -= BonusClickedHandler;
            this.winView.OnLoseBonusClicked -= LoseBonusClickedHandler;
            this.adsAdapter.RewardVideo.OnRewarded -= BonusRewardedHandler;
            this.adsAdapter.RewardVideo.OnShowFailed -= BonusShowFailedHandler;
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

        private void LogWinViewFlow(string nextLevel, string returnHome, string replay, bool hasAds)
        {
            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value;
            int level = currentLevelOrder + 1;
            int playIndex = PlayerPrefs.GetInt($"play_index_{level}", 0);

            Debug.Log(
                $"--- (TRACKING) win_view_flow | level: {level} | play_index: {playIndex} | level_mode: normal | next_level: {nextLevel} | return_home: {returnHome} | replay: {replay} | has_ads: {hasAds}");
            this.analyticTracker.LogEvent(new WinViewFlowEventData
            {
                eventName = WinViewFlowEventData.EVENT_NAME.win_view_flow,
                level = level.ToString(),
                level_mode = "normal",
                play_next_level = nextLevel,
                return_home = returnHome,
                replay = replay,
                has_ads = hasAds ? "true" : "false",
                play_index = playIndex
            });
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

            LogWinViewFlow("true", "false", "false", showAds);

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
                ShowChapterUnlockFlow().Forget();
                return;
            }

            this.eventPublisher.PublishAsync(new NextLevelClicked());
            Hide();
        }

        private async UniTaskVoid ShowChapterUnlockFlow()
        {
            Hide();

            var chapterUnlockPresenter = this.ScenePresenter.GetViewPresenter<ChapterUnlockPresenter>();
            bool chapterShown = await chapterUnlockPresenter.TryShowForNextChapterAndWait();

            if (chapterShown && chapterUnlockPresenter.WasBackHomeRequested)
                this.eventPublisher.PublishAsync(new BackHome());
            else
                this.eventPublisher.PublishAsync(new NextLevelClicked());
        }

        private void LoseBonusClickedHandler()
        {
            LogChapterBonusEvent(false);
            PlayDefaultRewardFlow().Forget();
        }

        private async UniTaskVoid PlayDefaultRewardFlow()
        {
            var ct = this.winView.GetShowCancellationToken();
            await this.winView.PlayDefaultRewardAndCloseAsync(ct);
            if (ct.IsCancellationRequested) return;
            this.adsAdapter.Mrec.Show(new AdPlacement("win_view"));
        }

        private async UniTaskVoid PlayBonusRewardFlow()
        {
            var ct = this.winView.GetShowCancellationToken();
            await this.winView.PlayBonusRewardAndCloseAsync(ct);
            if (ct.IsCancellationRequested) return;
            this.adsAdapter.Mrec.Show(new AdPlacement("win_view"));
        }

        private void LogChapterBonusEvent(bool useRewardBonus)
        {
            int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value;
            int level = currentLevelOrder + 1;
            string useRewardBonusParam = useRewardBonus ? "true" : "false";

            Debug.Log($"--- (TRACKING) Log Chapter Reward Event --- Level: {level} --- Use Reward: {useRewardBonusParam}");

            this.analyticTracker.LogEvent(new ChapterRewardEventData()
            {
                eventName = ChapterRewardEventData.EVENT_NAME.chapter_reward,
                level = level.ToString(),
                use_reward_bonus = useRewardBonusParam
            });
        }

        private void BonusClickedHandler()
        {
            if (this.adsAdapter.RewardVideo.IsReady)
            {
                var adReward = new AdReward("bonus_life_chapter");
                var adPlacement = new AdPlacement("chapter_bonus");
                this.adsAdapter.RewardVideo.Show(adReward, adPlacement);
            }
            else
            {
                ShowAdFailedDialog();
            }
        }

        private void BonusRewardedHandler(AdReward adReward)
        {
            string rewardId = adReward.RewardId;
            if (!rewardId.Equals("bonus_life_chapter"))
            {
                return;
            }

            int bonusAmount = this.remoteConfig.GetValue(ConfigKey.LifeBonusAfterChapter).Int;
            int defaultLifeReward = this.remoteConfig.GetValue(ConfigKey.LifeRecoverAfterChapter).Int;
            bonusAmount -= defaultLifeReward;

            this.lifeSystem.AddLives(bonusAmount, "chapter_bonus", rewardId);
            LogChapterBonusEvent(true);
            PlayBonusRewardFlow().Forget();
        }

        private void BonusShowFailedHandler(AdReward adReward, AdError adError)
        {
            string rewardId = adReward.RewardId;

            if (!rewardId.Equals("bonus_life_chapter"))
            {
                return;
            }

            ShowAdFailedDialog();
        }

        private void ReplayClickedHandler()
        {
            LogWinViewFlow("false", "false", "true", false);
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
            LogWinViewFlow("false", "true", "false", false);
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

        private void ShowAdFailedDialog()
        {
            if (this.dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide,
                    out AutoHideNotificationDialog dialog))
            {
                dialog.SetText("Ads is not available");
            }
        }
    }
}