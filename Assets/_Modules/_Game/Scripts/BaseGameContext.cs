using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using _Modules.Ads;
// using _Modules.Ads;
using Cysharp.Threading.Tasks;
using Economy.Resources;
using Firebase.Analytics;
using GoogleMobileAds.Api;
// using GoogleMobileAds.Api;
using Mimi.Ads.Adapters;
using Mimi.Ads.Adapters.Admob;
// using Mimi.Ads.Adapters.Admob;
using Mimi.Ads.Adapters.Extensions.Amazons.Maxs;
using Mimi.Ads.Adapters.Extensions.FirebaseAdRevenue;
using Mimi.Ads.Adapters.Extensions.SingularAdRevenue;
using Mimi.Ads.Adapters.Max;
using Mimi.Ads.Extensions.Requests;
using Mimi.Analytics.Sessions;
using Mimi.Analytics.Tracking.Firebase;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Configs;
using Mimi.DataSources.GoogleSheet;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Games.InitSteps;
using Mimi.Games.Plugins;
using Mimi.Games.ProjectConfigs;
using Mimi.Network.Monitors;
using Mimi.Persistence.LocalPrefs;
using Mimi.Prototypes.Pooling;
using Mimi.Prototypes.SaveLoad;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using Mimi.Services.ScriptableObject.Audio;
using Singular;
using UnityEngine;

namespace Mimi.Prototypes
{
    public abstract class BaseGameContext : BaseContext
    {
        [SerializeField] private SheetAsset gameDataAsset;
        [SerializeField] private SheetAsset localizeAsset;
        [SerializeField] private BaseAudioServiceSO audioService;
        [SerializeField] private DialogManager dialogManager;

        public RuntimeState RuntimeState { private set; get; }
        public bool IsAdmobConsentUpdateCompleted { private set; get; }
        public DialogManager DialogManager => this.dialogManager;
        public IResourceCollection PlayerResources { private set; get; }
        public ISessionRecorder SessionRecorder { private set; get; }
        public ISaveManager SaveManager { private set; get; }
        public IAsyncPublisher EventPublisher { private set; get; }
        public IAsyncSubscriber EventSubscriber { private set; get; }
        public IConfigProvider RemoteConfig { private set; get; }
        public IAdAdapter Ads { private set; get; }
        public IAnalyticTracker AnalyticTracker { private set; get; }
        public IAudioService AudioService { private set; get; }
        public ILocalizationService Localization { private set; get; }
        public ILocalPrefs LocalPrefs { private set; get; }
        public IInternetMonitor InternetMonitor { private set; get; }
        public GameData GameData { private set; get; }
        public ConsentHandler ConsentHandler { private set; get; }

        public LevelConfig RateConfig { get; } = new();
        public LevelConfig ShowInterstitialLevelConfig { protected set; get; }
        public bool IsRemoveAds => false;
        public bool IsRemoteConfigInitialized;

        private readonly CompositePlugin globalPluginContainer = new CompositePlugin();
        private IPluginConfigInjector projectPluginInjector;
        private MaxMrec maxMrec;
        private bool isMrecFirstSuccessLoad;

        private const string MaxSDKKey = "OBxrqJJrFUnTguh-MKCJDDMfXuiQUo_ALm8Eydwh70knZsGl3mLMVXR5UBsA_CSWI2gbdgRZl77STkOI0oJJhx";
        private const string TabletAmazonBannerId = "2e627403-846f-4f4e-8a28-24313ed5c55b";
        private const string PhoneAmazonUnitId = "f9c1c176-9bc7-41aa-ad4d-deb88828b696";
        private const string AdmobBannerId = "ca-app-pub-3485115086350845/9995674705";
        private const string AdmobAOAUnitId = "ca-app-pub-8798190451324475/4832373554";
        private const string AmazonMaxId = "39793f24-f3f0-481a-ad9a-c9f0d502106d";
        private const string AmazonInterUnitId = "9910d126-a213-456e-9f31-55b05ce74415";
        private const string AmazonRewardUnitId = "16d044a0-13af-4eb3-896c-538705396a12";
        private const string MaxAOAUnitId = "839ace4d3390b69e";
        private const string MaxInterUnityId = "a3941c80f707e5fb";
        private const string MaxRewardUnitId = "40177fea94ba8247";
        private const string MaxBannerUnitId = "3d6cf94b8b39a0b1";
        private const string MaxMrecUnitId = "b21d09db69c6a7a5";

        protected override async UniTask OnInitializing()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Application.targetFrameRate = 60;
            InitSheetAssets();
            this.projectPluginInjector = new UnityResourcePluginConfigInjector();
            IProjectConfigRepository projectConfigRepository = new ResourceProjectConfigRepository();
            ProjectConfig projectConfig = projectConfigRepository.Get();
            RuntimeState = RuntimeState.Get();
            await CreateCoreServices();
            SaveManager.Load();
            IGameInitiator gameInitiator = new ReportProgressGameInitiator(EventPublisher);
            AddDefaultInitSteps(gameInitiator, projectConfig);
            AddInitSteps(gameInitiator, projectConfig);
            await gameInitiator.Initialize();
            AddGlobalPlugins(this.globalPluginContainer);
            InjectPluginConfigs(this.globalPluginContainer.Plugins);
            await this.globalPluginContainer.Install();
            await this.globalPluginContainer.Begin();
        }

        public void InjectPluginConfigs(IEnumerable<IPlugin> plugins)
        {
            foreach (IPlugin plugin in plugins)
            {
                this.projectPluginInjector.Inject(plugin);
            }
        }

        protected List<TSheet> GetDataSheet<TSheet>(string sheetName)
        {
            return this.gameDataAsset.GetSheet<TSheet>(sheetName);
        }

        protected List<TSheet> GetDataSheet<TSheet>()
        {
            return this.gameDataAsset.GetSheet<TSheet>();
        }

        private void InitSheetAssets()
        {
            ISheetSerializer sheetSerializer = new SheetSerializerDefaultLit(new ModelConverterAot());
            this.gameDataAsset.Init(sheetSerializer);
            // this.localizeAsset.Init(sheetSerializer);
        }

        private void AddDefaultInitSteps(IGameInitiator gameInitiator, ProjectConfig projectConfig)
        {
        }

        public abstract void CreateServices();
        protected abstract void AddInitSteps(IGameInitiator gameInitiator, ProjectConfig projectConfig);
        protected abstract void AddGlobalPlugins(CompositePlugin pluginInstaller);

        private async UniTask CreateCoreServices()
        {
            await UniTask.CompletedTask;
            InitInternetMonitor();

            CreatePoolingService();
            CreateMessageServices();
            CreateLocalPrefsService();
            CreateGameSessionService();
            CreateDialogService();
            CreatePlayerResourceService();
            CreateGameData();
            CreateSaveService();
            CreateAnalyticService();
            CreateAudioService();

            await InitConfigService();
            LogInitializeEvent("init_config");
            await InitAdmobConsent();
            LogInitializeEvent("init_admob_consent");
            await InitGoogleMobileAds();
            LogInitializeEvent("init_gma");
            SingularSDK.InitializeSingularSDK();
            LogInitializeEvent("init_mmp");
            await InitAdsService();
            LogInitializeEvent("init_ads");
        }

        private void CreateAnalyticService()
        {
            AnalyticTracker = new ReflectionTracker(new FirebaseTrackingProvider());

#if UNITY_EDITOR
            AnalyticTracker = new NullTracker();
#endif
        }

        private void CreateGameData()
        {
            GameData = new GameData();
        }

        private void CreateLocalPrefsService()
        {
            LocalPrefs = new UnityPlayerPref();
        }

        private void CreateGameSessionService()
        {
            SessionRecorder = new LocalSessionRecorder(EventPublisher, LocalPrefs);
        }

        private void CreatePoolingService()
        {
            ServiceLocator.Global.Register<IPoolService>(new InternalPoolService());
        }

        private void CreateMessageServices()
        {
            EventSubscriber = AsyncMessageBus.Default;
            EventPublisher = AsyncMessageBus.Default;
            var messageServiceAdapter = new MessageServiceAdapter(EventPublisher, EventSubscriber);
            ServiceLocator.Global.Register<IEventService>(messageServiceAdapter);
        }

        private void CreateDialogService()
        {
            this.dialogManager.Initialize();
            ServiceLocator.Global.Register(this.dialogManager);
        }

        private void CreatePlayerResourceService()
        {
            var currencyRepo = new ResourceCollection();
            PlayerResources = currencyRepo;
            IResource coinResource = new Resource("Coin");
            PlayerResources.AddResource(coinResource);
        }

        private void CreateAudioService()
        {
            this.AudioService = new AudioServiceAdapter(this.audioService);
            ServiceLocator.Global.Register(this.AudioService);
        }

        private void CreateSaveService()
        {
            SaveManager = new ConvertibleSaveManager(this);
            SaveManager.AddSaveLoadStrategy(new GameSaver(this), new GameLoader(this));
        }

        private void InitInternetMonitor()
        {
            InternetMonitor = UnityInternetMonitor.New(5);
#if !UNITY_EDITOR
            InternetMonitor.OnNetworkStateChanged += NetworkStateChanged;
#endif
            InternetMonitor.StartMonitor();
        }

        private void NetworkStateChanged(InternetState internetState)
        {
            // if (!RemoteConfig.GetValue(ConfigKey.RequireInternet).Boolean) return;
            //
            // if (internetState == InternetState.Unavailable)
            // {
            //     if (this.showRequireInternetPopupHandler == default)
            //     {
            //         this.showRequireInternetPopupHandler = Timing.RunCoroutine(_ShowRequireInternetDialog());
            //     }
            // }
            // else if (internetState == InternetState.Available)
            // {
            //     if (this.showRequireInternetPopupHandler.IsValid)
            //     {
            //         Timing.KillCoroutines(this.showRequireInternetPopupHandler);
            //         this.showRequireInternetPopupHandler = default;
            //     }
            //
            //     if (this.requireInternetDialog != null)
            //     {
            //         this.requireInternetDialog.Hide();
            //         this.requireInternetDialog = null;
            //     }
            // }
        }

        protected void LogInitializeEvent(string eventName)
        {
            if (!this.SessionRecorder.IsFirstSession)
            {
                return;
            }

            // FirebaseAnalytics.LogEvent(eventName);
        }

        private async UniTask InitGoogleMobileAds()
        {
            if (Application.isEditor)
            {
                return;
            }

            if (IsRemoveAds)
            {
                return;
            }

            bool completed = false;
            MobileAds.Initialize(status => { completed = true; });

            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(10f));

            try
            {
                await UniTask.WaitUntil(() => completed, cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[GMA] Google Mobile Ads initialization timed out");
            }
            finally
            {
                cts.Dispose();
            }
        }

        private async UniTask InitAdmobConsent()
        {
            this.ConsentHandler = new ConsentHandler();
            await this.ConsentHandler.InitAdmobConsent();
            this.IsAdmobConsentUpdateCompleted = true;
        }

        private async UniTask InitAdsService()
        {
            if (Debug.isDebugBuild)
            {
                Ads = DebugAdAdapter.Instance;
                // Ads = new AdminToolAdapter(DebugAdAdapter.Instance);
                Ads.SetInterstitial(EditorInterstitialAdapter.Instance);
                Ads.SetRewardVideo(EditorRewardVideoAdapter.Instance);
                return;
            }

            MaxSdk.SetHasUserConsent(true);
            MaxSdk.SetDoNotSell(false);
            // SingularSDK.TrackingOptIn();

#if DEVELOPMENT
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => { MaxSdk.ShowMediationDebugger(); };
#endif

            // var amazonMaxAdapter = new AmazonMaxAdapter(AmazonMaxId, new MaxAdapter(MaxSDKKey, SystemInfo.deviceUniqueIdentifier));
            var maxAdapter = new MaxAdapter(MaxSDKKey, SystemInfo.deviceUniqueIdentifier);
            Ads = maxAdapter;

            var maxInitCts = new CancellationTokenSource();
            maxInitCts.CancelAfterSlim(TimeSpan.FromSeconds(30f));

            try
            {
                await Ads.Initialize().AttachExternalCancellation(maxInitCts.Token);
                Debug.Log($"[MAX] SDK initialized. GGAdmob ready: {MaxSdk.IsInitialized()}");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[MAX] AppLovin MAX SDK initialization timed out");
            }
            finally
            {
                maxInitCts.Dispose();
            }

            if (!IsRemoveAds)
            {
                var interstitialRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);
                var bannerRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);

                if (RemoteConfig.GetValue(ConfigKey.ShowBanner).Boolean)
                {
                    if (RemoteConfig.GetValue(ConfigKey.UseAdmobBanner).Boolean)
                    {
                        Ads.SetBanner(new AutoRequestBanner(bannerRequestStrategy,
                            // new FirebaseMeasureRevenueBanner(
                            new SingularRevenueBanner(
                                new AdmobBanner(AdmobBannerId))));
                    }
                    else
                    {
                        Ads.SetBanner(new AutoRequestBanner(bannerRequestStrategy,
                            // new FirebaseMeasureRevenueBanner(
                            new SingularRevenueBanner(
                                new AmazonMaxBanner(TabletAmazonBannerId,
                                    PhoneAmazonUnitId, MaxBannerUnitId))));
                    }
                }
                else
                {
                    Ads.SetBanner(NullBannerAdapter.Instance);
                }

                if (RemoteConfig.GetValue(ConfigKey.ShowInterstitial).Boolean)
                {
                    Ads.SetInterstitial(
                        new AutoRequestInterstitial(interstitialRequestStrategy,
                            // new FirebaseMeasureRevenueInterstitial(
                            new SingularRevenueInterstitial(
                                new MaxInterstitial(MaxInterUnityId))));
                }
                else
                {
                    Ads.SetInterstitial(EditorInterstitialAdapter.Instance);
                }

                if (RemoteConfig.GetValue(ConfigKey.ShowMREC).Boolean)
                {
                    CreateMrecWithCustomPosition();
                }
                else
                {
                    Ads.SetMrec(NullMrecAdapter.Instance);
                }

                var appOpenRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);

                if (RemoteConfig.GetValue(ConfigKey.UseMaxAoa).Boolean)
                {
                    Ads.SetAppOpen(
                        new AutoRequestAppOpen(appOpenRequestStrategy,
                            // new FirebaseMeasureRevenueAppOpen(
                            new SingularRevenueAppOpen(
                                new MaxAppOpen(MaxAOAUnitId))));
                }
                else
                {
                    Ads.SetAppOpen(
                        new AutoRequestAppOpen(appOpenRequestStrategy,
                            // new FirebaseMeasureRevenueAppOpen(
                            new SingularRevenueAppOpen(
                                new AdmobAppOpen(AdmobAOAUnitId))));
                }
            }
            else
            {
                Ads.SetInterstitial(EditorInterstitialAdapter.Instance);
                Ads.SetAppOpen(NullAppOpenAdapter.Instance);
                Ads.SetBanner(NullBannerAdapter.Instance);
                Ads.SetMrec(NullMrecAdapter.Instance);
            }

            if (RemoteConfig.GetValue(ConfigKey.ShowRewarded).Boolean)
            {
                var rewardVideoRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);
                Ads.SetRewardVideo(
                    new AutoRequestRewardVideo(rewardVideoRequestStrategy,
                        // new FirebaseMeasureRevenueRewardVideo(
                        new SingularRevenueRewardVideo(
                            new MaxRewardVideo(MaxRewardUnitId))));
            }
            else
            {
                Ads.SetRewardVideo(EditorRewardVideoAdapter.Instance);
            }

#if DEVELOPMENT
            if (PlayerPrefs.GetInt("RemoveAdsCheat", 0) != 0)
            {
                Ads.SetRewardVideo(EditorRewardVideoAdapter.Instance);
            }
#endif

            Ads.Banner.OnImpressionSuccess += AdsImpressionHandler;
            Ads.Interstitial.OnImpressionSuccess += AdsImpressionHandler;
            Ads.Mrec.OnImpressionSuccess += AdsImpressionHandler;
            Ads.RewardVideo.OnImpressionSuccess += AdsImpressionHandler;
            Ads.AppOpen.OnImpressionSuccess += AdsImpressionHandler;
            Ads.Mrec.OnLoadSucceeded += () =>
            {
                CalculateMrecPos();
                this.isMrecFirstSuccessLoad = true;
            };

            await EventPublisher.PublishAsync(new InitAdCompleted());
        }

        private static void AdsImpressionHandler(ImpressionData impressionData)
        {
            Parameter[] parameters =
            {
                new Parameter("ad_platform", "ApplovinMax"),
                new Parameter("ad_source", impressionData.AdNetwork),
                new Parameter("ad_format", impressionData.AdUnit),
                new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
                new Parameter(FirebaseAnalytics.ParameterValue, impressionData.Revenue)
            };
            // FirebaseAnalytics.LogEvent("ad_impression_mediation", parameters);
        }

        private void CreateMrecWithCustomPosition()
        {
            this.maxMrec = new MaxMrec(MaxMrecUnitId, 42, 484);
            var mrecRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);

            Ads.SetMrec(new AutoRequestMrec(mrecRequestStrategy,
                // new FirebaseAdRevenueMrec(
                new SingularRevenueMrec(this.maxMrec)));
        }

        private void CalculateMrecPos()
        {
            if (IsRemoveAds) return;
            var screenWidth = Screen.width / MaxSdkUtils.GetScreenDensity();
            var screenHeight = Screen.height / MaxSdkUtils.GetScreenDensity();
            var mrecRect = MaxSdk.GetMRecLayout(MaxMrecUnitId);
            int posX = 0;
            int posY = 0;

            if (!this.isMrecFirstSuccessLoad)
            {
                posX = Mathf.CeilToInt((screenWidth - 300) / 2);
                posY = Mathf.CeilToInt(screenHeight - 250 - 100f);
            }
            else
            {
                posX = Mathf.CeilToInt((screenWidth - mrecRect.width) / 2);
                posY = Mathf.CeilToInt(screenHeight - mrecRect.height - 100f);
            }

            this.maxMrec.SetPosition(posX, posY);
        }

        public async UniTask InitConfigService()
        {
            this.IsRemoteConfigInitialized = false;

#if !UNITY_EDITOR
            // RemoteConfig = new Mimi.Configs.Firebase.FirebaseConfigProvider(new PlayPrefCache());
                        RemoteConfig = NullConfigProvider.Instance;
#else
            RemoteConfig = NullConfigProvider.Instance;
#endif

            var blueprint = new ConfigBlueprint();
            blueprint
                .SetString(ConfigKey.LevelDevelopment, string.Empty)
                .SetString(ConfigKey.LevelProduction, string.Empty)
                .SetString(ConfigKey.ClientVersion, Application.version)
                .SetFloat(ConfigKey.AdCooldown, 60f)
                .SetString(ConfigKey.RateLevel, "5,35,65")
                .SetString(ConfigKey.HintLevel, "1,5")
                .SetBool(ConfigKey.IsShowAOA, true)
                .SetBool(ConfigKey.RequireInternet, true)
                .SetFloat(ConfigKey.InternetFailedDelay, 7f)
                .SetString(ConfigKey.ShowAdLevels, "10")
                .SetBool(ConfigKey.ResumeAds, true)
                .SetBool(ConfigKey.RatingPopup, true)
                .SetBool(ConfigKey.ShowAOA, true)
                .SetBool(ConfigKey.ShowAOAFirstOpen, false)
                .SetFloat(ConfigKey.CollapsibleCooldown, 30f)
                .SetBool(ConfigKey.ShowCollapAd, true)
                .SetBool(ConfigKey.ShowCollapAdManually, false)
                .SetBool(ConfigKey.ShowBanner, true)
                .SetBool(ConfigKey.ShowInterstitial, true)
                .SetBool(ConfigKey.ShowMREC, true)
                .SetBool(ConfigKey.ShowRewarded, true)
                .SetBool(ConfigKey.UseAdmobBanner, true)
                .SetBool(ConfigKey.UseMaxAoa, true)
                .SetString(ConfigKey.HardLevel, "10,20,30,40,50,60,70,80,90,100,110,120")
                .SetInt(ConfigKey.HardLevelBaseTime, 30)
                .SetInt(ConfigKey.HardLevelAdditionalTime, 60)
                .SetInt(ConfigKey.HardLevelWarningTime, 10)
                .SetInt(ConfigKey.CooldownInterAfterShowReward, 30);

            await RemoteConfig.SetDefaultValues(blueprint);

            this.RemoteConfig.OnFetchSuccess += () => { this.IsRemoteConfigInitialized = true; };

            this.RemoteConfig.OnFetchError += (configFetchError) =>
            {
                this.IsRemoteConfigInitialized = true;
                Debug.LogError($"[RemoteConfig] Fetching Error: " + configFetchError);
            };

            var timeOutSeconds = 4f;
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(timeOutSeconds));

            try
            {
                await this.RemoteConfig.Fetch();

                if (this.IsRemoteConfigInitialized)
                {
                    Debug.Log("[RemoteConfig] Firebase Remote Config Initialized immediately");
                }
                else
                {
                    await UniTask.WaitUntil(() => this.IsRemoteConfigInitialized, cancellationToken: cts.Token);
                    Debug.Log("[RemoteConfig] Firebase Remote Config Initialized before timeout");
                }
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    Debug.Log("[RemoteConfig] Firebase Remote Config Initialized Timeout");
                }

                Debug.LogException(ex);
            }
            finally
            {
                cts.Dispose();
                this.IsRemoteConfigInitialized = true;
            }
        }

        public void StopSound(string soundKey)
        {
            this.audioService.StopSound(soundKey);
        }

        protected override void OnPause(bool pause)
        {
            base.OnPause(pause);
            if (pause)
            {
                Time.timeScale = 0f;
                EventPublisher.PublishAsync(new GamePaused());
            }
            else
            {
                Time.timeScale = 1f;
                EventPublisher.PublishAsync(new GameUnpaused());
            }
        }
    }
}