using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Stopwatch = System.Diagnostics.Stopwatch;
using _Modules.Ads;
using Cysharp.Threading.Tasks;
using Economy.Resources;
using Firebase.Analytics;
using Games;
using GoogleMobileAds.Api;
using Mimi.Ads.Adapters;
using Mimi.Ads.Adapters.Admob;
using Mimi.Ads.Adapters.Extensions.AdminTools;
using Mimi.Ads.Adapters.Extensions.FirebaseAdRevenue;
using Mimi.Ads.Adapters.Extensions.SingularAdRevenue;
using Mimi.Ads.Adapters.Max;
using Mimi.Ads.Extensions.Requests;
using Mimi.Analytics.Sessions;
using Mimi.Analytics.Tracking.Firebase;
using Mimi.Analytics.Tracking.Trackers;
using Mimi.Audio;
using Mimi.Configs;
using Mimi.DataSources.GoogleSheet;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Games.InitSteps;
using Mimi.Games.Plugins;
using Mimi.Games.ProjectConfigs;
using Mimi.IAP;
using Mimi.IAP.Providers.Unity;
using Mimi.Network.Monitors;
using Mimi.Persistence.LocalPrefs;
using Mimi.Prototypes.Pooling;
using Mimi.Prototypes.SaveLoad;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using Mimi.Services.ScriptableObject.Audio;
using Singular;
using Tracking;
using UnityEngine;

namespace Mimi.Prototypes
{
    public abstract class BaseGameContext : BaseContext
    {
        [SerializeField] private SheetAsset gameDataAsset;
        [SerializeField] private SheetAsset localizeAsset;
        [SerializeField] private BaseAudioServiceSO audioService;
        [SerializeField] private DialogManager dialogManager;
        [SerializeField, SoundKey] private string bgmSoundKey;

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
        public IPurchasingProvider InAppPurchaseStore { private set; get; }
        public IAnalyticTracker AnalyticTracker { private set; get; }
        public IAudioService AudioService { private set; get; }
        public ILocalizationService Localization { private set; get; }
        public ILocalPrefs LocalPrefs { private set; get; }
        public IInternetMonitor InternetMonitor { private set; get; }
        public GameData GameData { private set; get; }
        public ConsentHandler ConsentHandler { private set; get; }
        public LevelConfig RateConfig { get; } = new();
        public LevelConfig ShowInterstitialLevelConfig { protected set; get; }
        public string RemoveAdsProductId { protected set; get; }
        public bool IsServiceInitialized { protected set; get; }
        public LifeSystem LifeSystem { protected set; get; }

        public bool IsRemoveAds
        {
            get
            {
                if (PlayerPrefs.GetInt("RemoveAds", 0) != 0)
                {
                    return true;
                }

                if (InAppPurchaseStore == null) return false;

                foreach (var product in InAppPurchaseStore.Products)
                {
                    if (product.Id.Equals(RemoveAdsProductId))
                    {
                        if (product.HasReceipt)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool IsRemoteConfigInitialized;
        public bool IsRemoteConfigLoadSuccess;
        public bool IsFirstSession => SessionRecorder.SessionCount <= 1;

        public string BGMSoundKey => this.bgmSoundKey;

        private readonly CompositePlugin globalPluginContainer = new CompositePlugin();
        private IPluginConfigInjector projectPluginInjector;
        private MaxMrec maxMrec;
        private bool isMrecFirstSuccessLoad;

        private const string MaxSDKKey = "PWIAAUJmCd5T2VvJ1XTAZiLr3pg1OC9wLSMuEuX8LOGdigNu3Ep6cUPtf5y5FxVUv8TrQJ64a_okXmyv0oTDNo";
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

        private UniTask configServiceTask;
        private UniTask consentLoadTask;
        private readonly Stopwatch initStopwatch = new Stopwatch();
        private readonly Stopwatch stepStopwatch = new Stopwatch();
        private long configServiceElapsedMs;

        protected override async UniTask OnInitializing()
        {
            this.initStopwatch.Restart();
            this.configServiceTask = InitConfigService();

            this.ConsentHandler = new ConsentHandler();
            this.consentLoadTask = this.ConsentHandler.LoadConsentAsync();

            this.projectPluginInjector = new UnityResourcePluginConfigInjector();

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            await UniTask.WaitUntil(() => BootLoader.IsBootViewReady);

#if RELEASE
            Debug.unityLogger.filterLogType = LogType.Exception;
#endif

            InitSheetAssets();
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
            LogInitializeEvent("init_analytic_service");
            SetUserProperties();
            CreateAudioService();
            LogInitializeEvent("init_audio_service");

            this.stepStopwatch.Restart();
            await WaitConfigService();
            LogInitializeEvent("init_config", this.stepStopwatch.ElapsedMilliseconds, this.configServiceElapsedMs);

            this.stepStopwatch.Restart();
            InitIAPService();
            LogInitializeEvent("init_iap", this.stepStopwatch.ElapsedMilliseconds);

            this.stepStopwatch.Restart();
            await ShowAdmobConsent();
            LogInitializeEvent("init_admob_consent", this.stepStopwatch.ElapsedMilliseconds);

            this.stepStopwatch.Restart();
            InitGoogleMobileAds().Forget();
            LogInitializeEvent("init_gma", this.stepStopwatch.ElapsedMilliseconds);

#if !UNITY_EDITOR
            this.stepStopwatch.Restart();
            HandleInitializingSingularSDK();
            LogInitializeEvent("init_mmp", this.stepStopwatch.ElapsedMilliseconds);
#endif

            this.stepStopwatch.Restart();
            UniTask adsInitTask = InitAdsService();
            await UniTask.WhenAny(adsInitTask, UniTask.Delay(TimeSpan.FromSeconds(5)));
            LogInitializeEvent("init_ads", this.stepStopwatch.ElapsedMilliseconds);

            InitLifeSystem();

            Debug.Log($"--- (INIT) CreateCoreServices total: {this.initStopwatch.ElapsedMilliseconds}ms");
        }

        private async UniTask WaitConfigService()
        {
            try
            {
                await this.configServiceTask;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Config] Config task failed: {ex.Message}");
                if (this.RemoteConfig == null) this.RemoteConfig = NullConfigProvider.Instance;
                this.IsRemoteConfigInitialized = true;
            }
        }

        private async UniTask HandleInitializingSingularSDK()
        {
            try
            {
                SingularSDK.InitializeSingularSDK();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MMP] Singular SDK init failed: {ex.Message}");
            }
        }

        private async UniTask ShowAdmobConsent()
        {
            await this.consentLoadTask;
            this.ConsentHandler.ShowConsentIfNeeded();
            this.IsAdmobConsentUpdateCompleted = true;
        }

        private void CreateAnalyticService()
        {
            AnalyticTracker = new TypeAwareTracker(new FirebaseTrackingProvider());
#if UNITY_EDITOR
            AnalyticTracker = new TestAnalyticTracker(new TestTrackingProvider());
#endif
            this.AnalyticTracker.LogEvent(new Feature_LOADING_START()
            {
                eventName = Feature_LOADING_START.EVENT_NAME.loading_start,
                placement = "app_open"
            });
        }

        private void SetUserProperties()
        {
            IUserPropertyData userProperty = new USER_PROPERTIES()
            {
                user_properties = USER_PROPERTIES_TYPE.current_level,
                value = "0"
            };
            this.AnalyticTracker.SetUserProperties(userProperty);
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
            SessionRecorder.RecordSessionStart(DateTime.UtcNow);
            Debug.Log($"--- (INIT) Session count: {SessionRecorder.SessionCount} --> IsFirstSession: {IsFirstSession}");
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
            IResource lifeResource = new Resource("Life");
            PlayerResources.AddResource(lifeResource);
        }

        private void CreateAudioService()
        {
            this.AudioService = new AudioServiceAdapter(this.audioService);
            ServiceLocator.Global.Register(this.AudioService);
            HandleFirstAudio();
        }

        private void CreateSaveService()
        {
            SaveManager = new ConvertibleSaveManager(this);
            SaveManager.AddSaveLoadStrategy(new GameSaver(this), new GameLoader(this));
        }

        private async UniTask InitIAPService()
        {
            RemoveAdsProductId = RemoteConfig.GetValue(ConfigKey.RemoveAdProductId).String;

            InAppPurchaseStore = new UnityPurchasingProvider(new MockPurchaseValidator());
            InAppPurchaseStore.Initialize(new[]
            {
                new ProductMetadata(RemoveAdsProductId, ProductType.NonConsumable),
            });
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
            if (internetState != InternetState.Available)
            {
                return;
            }

            if (this.Ads == null)
            {
                return;
            }

            if (IsRemoveAds)
            {
                this.Ads.RewardVideo.Load();
            }
            else
            {
                this.Ads.Interstitial.Load();
                this.Ads.RewardVideo.Load();
                this.Ads.Mrec.Load();
                this.Ads.AppOpen.Load();
                this.Ads.Banner.Load(new AdPlacement("Bottom"), BannerSize.Adaptive, BannerPosition.Bottom);
            }
        }

        protected void LogInitializeEvent(string eventName, long elapsedMs = -1, long actualDurationMs = -1)
        {
            long totalMs = this.initStopwatch.ElapsedMilliseconds;
            if (elapsedMs >= 0 && actualDurationMs >= 0)
            {
                Debug.Log($"--- (BOOT) {eventName} blocked={elapsedMs}ms actual={actualDurationMs}ms (total {totalMs}ms)");
            }
            else if (elapsedMs >= 0)
            {
                Debug.Log($"--- (BOOT) {eventName} took {elapsedMs}ms (total {totalMs}ms)");
            }
            else
            {
                Debug.Log($"--- (BOOT) Initializing {eventName} (total {totalMs}ms)");
            }

            if (!this.IsFirstSession)
            {
                return;
            }

            FirebaseAnalytics.LogEvent(eventName);
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
            float gmaTimeOutSec = 2f;
            cts.CancelAfterSlim(TimeSpan.FromSeconds(gmaTimeOutSec));
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
                cts.Cancel();
                cts.Dispose();
            }
        }

        private async UniTask InitAdsService()
        {
            if (Debug.isDebugBuild)
            {
                Ads = new AdminToolAdapter(DebugAdAdapter.Instance);
                // Ads = new AdminToolAdapter(DebugAdAdapter.Instance);
                var interstitialRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);
                var interCooldown = RemoteConfig.GetValue(ConfigKey.AdCooldown).Float;
                Ads.SetInterstitial(
                    new ThrottleInterstitialByCooldown(
                        new AutoRequestInterstitial(interstitialRequestStrategy, EditorInterstitialAdapter.Instance), interCooldown, this.GameData));
                Ads.SetRewardVideo(EditorRewardVideoAdapter.Instance);
                return;
            }

            MaxSdk.SetHasUserConsent(true);
            MaxSdk.SetDoNotSell(false);
            SingularSDK.TrackingOptIn();

#if DEVELOPMENT
            MaxSdk.SetVerboseLogging(true);
            MaxSdk.SetCreativeDebuggerEnabled(true);

            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration => { MaxSdk.ShowMediationDebugger(); };
#endif

            Debug.Log($"--- (ADS) Ads initializing...");

            // var amazonMaxAdapter = new AmazonMaxAdapter(AmazonMaxId, new MaxAdapter(MaxSDKKey, SystemInfo.deviceUniqueIdentifier));
            var maxAdapter = new AdminToolAdapter(new MaxAdapter(MaxSDKKey, SystemInfo.deviceUniqueIdentifier));
            Ads = maxAdapter;
            await Ads.Initialize();

            if (!IsRemoveAds)
            {
                var interstitialRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);
                var bannerRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);

                if (RemoteConfig.GetValue(ConfigKey.ShowBanner).Boolean)
                {
                    if (RemoteConfig.GetValue(ConfigKey.UseAdmobBanner).Boolean)
                    {
                        Debug.Log($"--- (ADS) Admob Banner Ads initializing...");
                        Ads.SetBanner(new AutoRequestBanner(bannerRequestStrategy,
                            new FirebaseMeasureRevenueBanner(
                                new SingularRevenueBanner(
                                    new AdmobBanner(AdmobBannerId)))));
                    }
                    else
                    {
                        Ads.SetBanner(new AutoRequestBanner(bannerRequestStrategy,
                            new FirebaseMeasureRevenueBanner(
                                new SingularRevenueBanner(
                                    new MaxBanner(MaxBannerUnitId)))));
                    }
                }
                else
                {
                    Ads.SetBanner(NullBannerAdapter.Instance);
                }

                if (RemoteConfig.GetValue(ConfigKey.ShowInterstitial).Boolean)
                {
                    Debug.Log($"--- (ADS) Inter Ads initializing...");
                    var interCooldown = RemoteConfig.GetValue(ConfigKey.AdCooldown).Float;
                    Ads.SetInterstitial(
                        new ThrottleInterstitialByCooldown(
                            new AutoRequestInterstitial(interstitialRequestStrategy,
                                new FirebaseMeasureRevenueInterstitial(
                                    new SingularLogInterstitial(
                                        new SingularRevenueInterstitial(
                                            new MaxInterstitial(MaxInterUnityId))))), interCooldown, this.GameData));
                }
                else
                {
                    Ads.SetInterstitial(EditorInterstitialAdapter.Instance);
                }

                if (RemoteConfig.GetValue(ConfigKey.ShowMREC).Boolean)
                {
                    CreateMrecWithCustomPosition();
                    Ads.Mrec.OnLoadSucceeded += () =>
                    {
                        CalculateMrecPos();
                        this.isMrecFirstSuccessLoad = true;
                    };
                }
                else
                {
                    Ads.SetMrec(NullMrecAdapter.Instance);
                }

                var appOpenRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);

                if (RemoteConfig.GetValue(ConfigKey.UseMaxAoa).Boolean)
                {
                    Debug.Log($"--- (ADS) MAX AOA Ads initializing...");

                    Ads.SetAppOpen(
                        new AutoRequestAppOpen(appOpenRequestStrategy,
                            new FirebaseMeasureRevenueAppOpen(
                                new SingularRevenueAppOpen(
                                    new MaxAppOpen(MaxAOAUnitId)))));
                }
                else
                {
                    // Ads.SetAppOpen(
                    //     new AutoRequestAppOpen(appOpenRequestStrategy,
                    //         new FirebaseMeasureRevenueAppOpen(
                    //             new SingularRevenueAppOpen(
                    //                 new AdmobAppOpen(AdmobAOAUnitId)))));
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
                Debug.Log($"--- (ADS) Rewarded Ads initializing...");

                var rewardVideoRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);
                Ads.SetRewardVideo(
                    new AutoRequestRewardVideo(rewardVideoRequestStrategy,
                        new FirebaseMeasureRevenueRewardVideo(
                            new SingularLogRewardVideo(
                                new SingularRevenueRewardVideo(
                                    new MaxRewardVideo(MaxRewardUnitId))))));
            }
            else
            {
                Ads.SetRewardVideo(EditorRewardVideoAdapter.Instance);
            }

            Ads.Banner.OnImpressionSuccess += AdsImpressionHandler;
            Ads.Interstitial.OnImpressionSuccess += AdsImpressionHandler;
            Ads.Mrec.OnImpressionSuccess += AdsImpressionHandler;
            Ads.RewardVideo.OnImpressionSuccess += AdsImpressionHandler;
            Ads.AppOpen.OnImpressionSuccess += AdsImpressionHandler;

            Ads.AppOpen.Load();
            await EventPublisher.PublishAsync(new InitAdCompleted());
        }

        private static void AdsImpressionHandler(ImpressionData impressionData)
        {
            // Parameter[] parameters =
            // {
            //     new Parameter("ad_platform", "ApplovinMax"),
            //     new Parameter("ad_source", impressionData.AdNetwork),
            //     new Parameter("ad_format", impressionData.AdUnit),
            //     new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
            //     new Parameter(FirebaseAnalytics.ParameterValue, impressionData.Revenue)
            // };
            // FirebaseAnalytics.LogEvent("ad_impression_mediation", parameters);
        }

        private void CreateMrecWithCustomPosition()
        {
            Debug.Log($"--- (ADS) MREC Ads initializing...");

            this.maxMrec = new MaxMrec(MaxMrecUnitId, 42, 484);
            var mrecRequestStrategy = new ExponentialCooldown(999, 2, InternetMonitor);

            Ads.SetMrec(new AutoRequestMrec(mrecRequestStrategy,
                new FirebaseAdRevenueMrec(
                    new SingularRevenueMrec(this.maxMrec))));
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
            var configStopwatch = Stopwatch.StartNew();
            this.IsRemoteConfigInitialized = false;
            this.IsRemoteConfigLoadSuccess = false;

#if !UNITY_EDITOR
            RemoteConfig = new Mimi.Configs.Firebase.FirebaseConfigProvider(new PlayPrefCache());
#else
            RemoteConfig = NullConfigProvider.Instance;
#endif

            var blueprint = new ConfigBlueprint();
            blueprint
                .SetString(ConfigKey.LevelDevelopment,
                    "Order,Id,Chapter 1,L_001,1 2,L_002,1 3,L_003,1 4,L_061,1 5,L_029,1 6,L_032,1 7,L_015,2 8,L_021,2 9,L_022,2 10,L_049,2 11,L_019,2 12,L_104,2 13,L_004,3 14,L_017,3 15,L_045,3 16,L_024,3 17,L_151,3 18,L_014,3 19,L_028,3 20,L_053,4 21,L_033,4 22,L_006,4 23,L_020,4 24,L_099,4 25,L_027,4 26,L_047,4 27,L_044,5 28,L_050,5 29,L_007,5 30,L_016,5 31,L_018,5 32,L_031,5 33,L_034,5 34,L_037,5 35,L_040,5 36,L_162,5 37,L_056,5 38,L_132,6 39,L_103,6 40,L_068,6 41,L_079,6 42,L_071,6 43,L_076,6 44,L_078,6 45,L_012,7 46,L_043,7 47,L_055,7 48,L_110,7 49,L_119,7 50,L_126,7 51,L_150,7 52,L_005,8 53,L_060,8 54,L_052,8 55,L_025,8 56,L_062,8 57,L_115,8 58,L_160,8 59,L_105,8 60,L_065,8 61,L_093,8 62,L_067,8 63,L_057,8 64,L_077,9 65,L_070,9 66,L_080,9 67,L_082,9 68,L_163,9 69,L_081,9 70,L_107,10 71,L_036,10 72,L_038,10 73,L_064,10 74,L_102,10 75,L_008,11 76,L_011,11 77,L_035,11 78,L_039,11 79,L_054,11 80,L_072,11 81,L_114,11 82,L_074,11 83,L_075,12 84,L_069,12 85,L_096,12 86,L_026,12 87,L_111,12 88,L_113,12 89,L_106,12 90,L_116,12 91,L_117,12 92,L_118,12 93,L_120,12 94,L_121,12 95,L_073,12 96,L_112,12 97,L_010,12 98,L_086,12 99,L_013,12 100,L_084,13 101,L_087,13 102,L_088,13 103,L_089,13 104,L_109,13 105,L_108,13 106,L_136,13 107,L_145,13 108,L_147,13 109,L_201,13 110,L_204,13 111,L_216,13 112,L_217,13 113,L_226,13 114,L_263,13 115,L_267,13 116,L_207,13 117,L_215,13")
                .SetString(ConfigKey.LevelProduction,
                    "Order,Id,Chapter 1,L_001,1 2,L_002,1 3,L_003,1 4,L_061,1 5,L_029,1 6,L_032,1 7,L_015,2 8,L_021,2 9,L_022,2 10,L_049,2 11,L_019,2 12,L_104,2 13,L_004,3 14,L_017,3 15,L_045,3 16,L_024,3 17,L_151,3 18,L_014,3 19,L_028,3 20,L_053,4 21,L_033,4 22,L_006,4 23,L_020,4 24,L_099,4 25,L_027,4 26,L_047,4 27,L_044,5 28,L_050,5 29,L_007,5 30,L_016,5 31,L_018,5 32,L_031,5 33,L_034,5 34,L_037,5 35,L_040,5 36,L_162,5 37,L_056,5 38,L_132,6 39,L_103,6 40,L_068,6 41,L_079,6 42,L_071,6 43,L_076,6 44,L_078,6 45,L_012,7 46,L_043,7 47,L_055,7 48,L_110,7 49,L_119,7 50,L_126,7 51,L_150,7 52,L_005,8 53,L_060,8 54,L_052,8 55,L_025,8 56,L_062,8 57,L_115,8 58,L_160,8 59,L_105,8 60,L_065,8 61,L_093,8 62,L_067,8 63,L_057,8 64,L_077,9 65,L_070,9 66,L_080,9 67,L_082,9 68,L_163,9 69,L_081,9 70,L_107,10 71,L_036,10 72,L_038,10 73,L_064,10 74,L_102,10 75,L_008,11 76,L_011,11 77,L_035,11 78,L_039,11 79,L_054,11 80,L_072,11 81,L_114,11 82,L_074,11 83,L_075,12 84,L_069,12 85,L_096,12 86,L_026,12 87,L_111,12 88,L_113,12 89,L_106,12 90,L_116,12 91,L_117,12 92,L_118,12 93,L_120,12 94,L_121,12 95,L_073,12 96,L_112,12 97,L_010,12 98,L_086,12 99,L_013,12 100,L_084,13 101,L_087,13 102,L_088,13 103,L_089,13 104,L_109,13 105,L_108,13 106,L_136,13 107,L_145,13 108,L_147,13 109,L_201,13 110,L_204,13 111,L_216,13 112,L_217,13 113,L_226,13 114,L_263,13 115,L_267,13 116,L_207,13 117,L_215,13")
                .SetString(ConfigKey.ChapterDevelopment,
                    "Id,ChapterName,ChapterIconAddress 1,Spreading Kindness,chapter1b 2,Love Blooming,chapter2b 3,Family Bonding,chapter3b 4,Facing Fear,chapter4b 5,Warming Hearts,chapter5b 6,Discovering,chapter6 7,Love Drama,chapter7 8,Helping Hands,chapter8 9,Exploring,chapter9 10,Nurturing Love,chapter10 11,Loving Animals,chapter11 12,Healing Hearts,chapter12 13,Uncovering Secrets,Chapter9")
                .SetString(ConfigKey.ChapterProduction,
                    "Id,ChapterName,ChapterIconAddress 1,Spreading Kindness,chapter1b 2,Love Blooming,chapter2b 3,Family Bonding,chapter3b 4,Facing Fear,chapter4b 5,Warming Hearts,chapter5b 6,Discovering,chapter6 7,Love Drama,chapter7 8,Helping Hands,chapter8 9,Exploring,chapter9 10,Nurturing Love,chapter10 11,Loving Animals,chapter11 12,Healing Hearts,chapter12 13,Uncovering Secrets,Chapter9")
                .SetString(ConfigKey.ClientVersion, Application.version)
                .SetFloat(ConfigKey.AdCooldown, 120f)
                .SetString(ConfigKey.RateLevel, "30")
                .SetString(ConfigKey.HintLevel, "1")
                .SetBool(ConfigKey.IsShowAOA, false)
                .SetBool(ConfigKey.RequireInternet, true)
                .SetBool(ConfigKey.ShowNextChapterInWinView, true)
                .SetFloat(ConfigKey.InternetFailedDelay, 7f)
                .SetString(ConfigKey.ShowAdLevels, "20")
                .SetString(ConfigKey.RemoveAdProductId, "removeads_199")
                .SetBool(ConfigKey.ResumeAds, false)
                .SetBool(ConfigKey.RatingPopup, true)
                .SetBool(ConfigKey.ShowAOAFirstOpen, false)
                .SetFloat(ConfigKey.CollapsibleCooldown, 30f)
                .SetBool(ConfigKey.ShowCollapAd, true)
                .SetBool(ConfigKey.ShowCollapAdManually, false)
                .SetBool(ConfigKey.ShowBanner, true)
                .SetBool(ConfigKey.ShowInterstitial, true)
                .SetBool(ConfigKey.ShowMREC, false)
                .SetBool(ConfigKey.ShowRewarded, true)
                .SetBool(ConfigKey.UseAdmobBanner, false)
                .SetBool(ConfigKey.UseMaxAoa, true)
                .SetString(ConfigKey.HardLevel, "10,20,30,38,50,61,72,81,91,100,110,120")
                .SetInt(ConfigKey.HardLevelBaseTime, 30)
                .SetInt(ConfigKey.HardLevelAdditionalTime, 30)
                .SetInt(ConfigKey.HardLevelWarningTime, 10)
                .SetFloat(ConfigKey.CooldownInterAfterShowReward, 30)
                .SetInt(ConfigKey.LifeCooldown, 900)
                .SetInt(ConfigKey.MaxLife, 10)
                .SetInt(ConfigKey.LifeRecoverAfterChapter, 2)
                .SetInt(ConfigKey.LifeAddAfterReward, 1)
                .SetInt(ConfigKey.ShowHintButtonAfterWrongTimes, 1)
                .SetInt(ConfigKey.ShowSkipButtonAfterWrongTimes, 2)
                .SetInt(ConfigKey.ShowObjectHintAfterWrongTimes, 1)
                .SetFloat(ConfigKey.HintButtonDelay, 3f)
                .SetFloat(ConfigKey.SkipButtonDelay, 7f)
                .SetFloat(ConfigKey.ObjectHintDelay, 20f)
                .SetBool(ConfigKey.ShowTutorialUI, false);

            await RemoteConfig.SetDefaultValues(blueprint);

            this.RemoteConfig.OnFetchSuccess += () =>
            {
                this.IsRemoteConfigInitialized = true;
                this.IsRemoteConfigLoadSuccess = true;
                Debug.Log($"--- (CONFIG) {ConfigKey.LevelDevelopment}: {RemoteConfig.GetValue(ConfigKey.LevelDevelopment).String}");
                Debug.Log($"--- (CONFIG) {ConfigKey.LevelProduction}: {RemoteConfig.GetValue(ConfigKey.LevelProduction).String}");
                Debug.Log($"--- (CONFIG) {ConfigKey.ChapterProduction}: {RemoteConfig.GetValue(ConfigKey.ChapterProduction).String}");
                Debug.Log($"--- (CONFIG) {ConfigKey.ChapterDevelopment}: {RemoteConfig.GetValue(ConfigKey.ChapterDevelopment).String}");
                Debug.Log($"--- (CONFIG) {ConfigKey.ShowAdLevels}: {RemoteConfig.GetValue(ConfigKey.ShowAdLevels).String}");
                Debug.Log($"--- (CONFIG) {ConfigKey.HardLevel}: {RemoteConfig.GetValue(ConfigKey.HintLevel).String}");
                Debug.Log($"--- (CONFIG) {ConfigKey.HardLevel}: {RemoteConfig.GetValue(ConfigKey.HardLevel).String}");
                Debug.Log($"--- (CONFIG) {ConfigKey.HardLevelBaseTime}: {RemoteConfig.GetValue(ConfigKey.HardLevelBaseTime).Int}");
                Debug.Log($"--- (CONFIG) {ConfigKey.HardLevelAdditionalTime}: {RemoteConfig.GetValue(ConfigKey.HardLevelAdditionalTime).Int}");
                Debug.Log($"--- (CONFIG) {ConfigKey.HardLevelWarningTime}: {RemoteConfig.GetValue(ConfigKey.HardLevelWarningTime).Int}");
                Debug.Log($"--- (CONFIG) {ConfigKey.MaxLife}: {RemoteConfig.GetValue(ConfigKey.MaxLife).Int}");
                Debug.Log($"--- (CONFIG) {ConfigKey.LifeCooldown}: {RemoteConfig.GetValue(ConfigKey.LifeCooldown).Int}");
                Debug.Log($"--- (CONFIG) {ConfigKey.AdCooldown}: {RemoteConfig.GetValue(ConfigKey.AdCooldown).Float}");
            };

            this.RemoteConfig.OnFetchError += (configFetchError) =>
            {
                this.IsRemoteConfigInitialized = true;
                this.IsRemoteConfigLoadSuccess = false;
                Debug.LogError($"[RemoteConfig] Fetching Error: " + configFetchError);
            };

            var timeOutSeconds = 4f;
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(timeOutSeconds));

            try
            {
                this.RemoteConfig.Fetch();
                await UniTask.WaitUntil(() => this.IsRemoteConfigInitialized, cancellationToken: cts.Token);
                Debug.Log("[RemoteConfig] Firebase Remote Config Initialized before timeout");
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    Debug.Log("[RemoteConfig] Firebase Remote Config Initialized Timeout");
                }

                this.IsRemoteConfigLoadSuccess = false;
                Debug.LogException(ex);
            }
            finally
            {
                cts.Cancel();
                cts.Dispose();
                this.IsRemoteConfigInitialized = true;
                configStopwatch.Stop();
                this.configServiceElapsedMs = configStopwatch.ElapsedMilliseconds;
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

        private void HandleFirstAudio()
        {
            this.AudioService.PlaySound(this.BGMSoundKey);
            this.AudioService.SetMusicVolPercentage(this.GameData.SettingModel.MusicOn ? 1 : 0);
            this.AudioService.SetSoundVolPercentage(this.GameData.SettingModel.SoundOn ? 1 : 0);

            // Debug.Log($"--- (Audio) MusicOn: {this.GameData.SettingModel.MusicOn} --- {this.AudioService.MusicVolPercentage}");
            // Debug.Log($"--- (Audio) SoundOn: {this.GameData.SettingModel.SoundOn} --- {this.AudioService.SoundVolPercentage}");
        }

        private void InitLifeSystem()
        {
            var lifeCooldown = this.RemoteConfig.GetValue(ConfigKey.LifeCooldown).Int;
            var maxLife = this.RemoteConfig.GetValue(ConfigKey.MaxLife).Int;
            LifeSystem = new LifeSystem(maxLife, lifeCooldown, this.PlayerResources, this.EventPublisher, this.EventSubscriber, this.DialogManager, this.Ads, this.RemoteConfig);
            LogInitializeEvent("init_life_system");
        }
    }
}