using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using _Modules.Ads;
using Cysharp.Threading.Tasks;
using Economy.Resources;
using Firebase.Analytics;
using Games;
using GoogleMobileAds.Api;
using Mimi.Ads.Adapters;
using Mimi.Ads.Adapters.Admob;
using Mimi.Ads.Adapters.Extensions.AdminTools;
using Mimi.Ads.Adapters.Extensions.Amazons.Maxs;
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

        public bool IsRemoveAds
        {
            get
            {
#if DEVELOPMENT
                if (PlayerPrefs.GetInt("RemoveAdsCheat", 0) != 0)
                {
                    return true;
                }

                foreach (var product in InAppPurchaseStore.Products)
                {
                    if (product.Id.Equals(ProductKey.RemoveAds_Android))
                    {
                        if (product.HasReceipt)
                        {
                            return true;
                        }
                    }
                }
#else
                foreach (var product in InAppPurchaseStore.Products)
                {
                    if (product.Id.Equals(ProductKey.RemoveAds_Android))
                    {
                        if (product.HasReceipt)
                        {
                            return true;
                        }
                    }
                }
#endif
                return false;
            }
        }

        public bool IsRemoteConfigInitialized;
        public bool IsFirstSession => SessionRecorder.SessionCount <= 1;

        public string BGMSoundKey => this.bgmSoundKey;

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

        private UniTask configServiceTask;

        protected override async UniTask OnInitializing()
        {
            this.configServiceTask = InitConfigService();

            await UniTask.WaitUntil(() => BootLoader.IsBootViewReady);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Application.targetFrameRate = 60;

#if RELEASE
            Debug.unityLogger.filterLogType = LogType.Exception;
#endif

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
            LogInitializeEvent("init_analytic_service");
            CreateAudioService();
            LogInitializeEvent("init_audio_service");

            await this.configServiceTask;
            LogInitializeEvent("init_config");
            await InitIAPService();
            LogInitializeEvent("init_iap");
            await InitAdmobConsent();
            LogInitializeEvent("init_admob_consent");
            await InitGoogleMobileAds();
            LogInitializeEvent("init_gma");
#if !UNITY_EDITOR
            SingularSDK.InitializeSingularSDK();
            LogInitializeEvent("init_mmp");
#endif
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
            InAppPurchaseStore = new UnityPurchasingProvider(new MockPurchaseValidator());
            InAppPurchaseStore.Initialize(new[]
            {
                new ProductMetadata(ProductKey.RemoveAds_Android, ProductType.NonConsumable),
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
            Debug.Log($"--- (INIT) Initializing {eventName}");
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
            cts.CancelAfterSlim(TimeSpan.FromSeconds(5f));
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
                    Ads.SetAppOpen(
                        new AutoRequestAppOpen(appOpenRequestStrategy,
                            new FirebaseMeasureRevenueAppOpen(
                                new SingularRevenueAppOpen(
                                    new AdmobAppOpen(AdmobAOAUnitId)))));
                }
            }
            else
            {
                Ads.SetInterstitial(EditorInterstitialAdapter.Instance);
                Ads.SetAppOpen(NullAppOpenAdapter.Instance);
                Ads.SetBanner(NullBannerAdapter.Instance);
                Ads.SetMrec(NullMrecAdapter.Instance);
            }

            Ads.AppOpen.Load();

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
            FirebaseAnalytics.LogEvent("ad_impression_mediation", parameters);
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
            this.IsRemoteConfigInitialized = false;

#if !UNITY_EDITOR
            RemoteConfig = new Mimi.Configs.Firebase.FirebaseConfigProvider(new PlayPrefCache());
#else
            RemoteConfig = NullConfigProvider.Instance;
#endif

            var blueprint = new ConfigBlueprint();
            blueprint
                .SetString(ConfigKey.LevelDevelopment,
                    "Order,Id,Chapter 1,L_001,1 2,L_049,1 3,L_043,1 4,L_012,1 5,L_021,1 6,L_022,1 7,L_036,1 8,L_038,1 9,L_015,1 10,L_019,1 11,L_002,2 12,L_005,2 13,L_007,2 14,L_013,2 15,L_016,2 16,L_018,2 17,L_025,2 18,L_026,2 19,L_029,2 20,L_031,2 21,L_004,3 22,L_014,3 23,L_017,3 24,L_024,3 25,L_028,3 26,L_045,3 27,L_106,3 28,L_151,3 29,L_068,4 30,L_070,4 31,L_071,4 32,L_076,4 33,L_077,4 34,L_078,4 35,L_079,4 36,L_080,4 37,L_081,4 38,L_082,4 39,L_103,4 40,L_163,4 41,L_132,4 42,L_055,5 43,L_061,5 44,L_064,5 45,L_102,5 46,L_104,5 47,L_107,5 48,L_110,5 49,L_119,5 50,L_126,5 51,L_150,5 52,L_008,6 53,L_011,6 54,L_035,6 55,L_039,6 56,L_054,6 57,L_072,6 58,L_114,6 59,L_074,6 60,L_162,6 61,L_032,7 62,L_034,7 63,L_037,7 64,L_040,7 65,L_044,7 66,L_050,7 67,L_056,7 68,L_060,7 69,L_062,7 70,L_065,7 71,L_003,8 72,L_006,8 73,L_010,8 74,L_020,8 75,L_027,8 76,L_033,8 77,L_047,8 78,L_052,8 79,L_053,8 80,L_057,8 81,L_099,8 82,L_112,8 83,L_069,8 84,L_067,9 85,L_075,9 86,L_093,9 87,L_096,9 88,L_105,9 89,L_111,9 90,L_113,9 91,L_115,9 92,L_116,9 93,L_117,9 94,L_118,9 95,L_120,9 96,L_121,9 97,L_160,9 98,L_086,9 99,L_073,9")
                .SetString(ConfigKey.LevelProduction,
                    "Order,Id,Chapter 1,L_001,1 2,L_049,1 3,L_043,1 4,L_012,1 5,L_021,1 6,L_022,1 7,L_036,1 8,L_038,1 9,L_015,1 10,L_019,1 11,L_002,2 12,L_005,2 13,L_007,2 14,L_013,2 15,L_016,2 16,L_018,2 17,L_025,2 18,L_026,2 19,L_029,2 20,L_031,2 21,L_004,3 22,L_014,3 23,L_017,3 24,L_024,3 25,L_028,3 26,L_045,3 27,L_106,3 28,L_151,3 29,L_068,4 30,L_070,4 31,L_071,4 32,L_076,4 33,L_077,4 34,L_078,4 35,L_079,4 36,L_080,4 37,L_081,4 38,L_082,4 39,L_103,4 40,L_163,4 41,L_132,4 42,L_055,5 43,L_061,5 44,L_064,5 45,L_102,5 46,L_104,5 47,L_107,5 48,L_110,5 49,L_119,5 50,L_126,5 51,L_150,5 52,L_008,6 53,L_011,6 54,L_035,6 55,L_039,6 56,L_054,6 57,L_072,6 58,L_114,6 59,L_074,6 60,L_162,6 61,L_032,7 62,L_034,7 63,L_037,7 64,L_040,7 65,L_044,7 66,L_050,7 67,L_056,7 68,L_060,7 69,L_062,7 70,L_065,7 71,L_003,8 72,L_006,8 73,L_010,8 74,L_020,8 75,L_027,8 76,L_033,8 77,L_047,8 78,L_052,8 79,L_053,8 80,L_057,8 81,L_099,8 82,L_112,8 83,L_069,8 84,L_067,9 85,L_075,9 86,L_093,9 87,L_096,9 88,L_105,9 89,L_111,9 90,L_113,9 91,L_115,9 92,L_116,9 93,L_117,9 94,L_118,9 95,L_120,9 96,L_121,9 97,L_160,9 98,L_086,9 99,L_073,9")
                .SetString(ConfigKey.ChapterDevelopment,
                    "Id,ChapterName,ChapterIconAddress 1,Love I,Chapter1 2,Help I,Chapter2 3,Family,Chapter3 4,Discovery,Chapter4 5,Love II,Chapter5 6,Animals,Chapter6 7,Help II,Chapter7 8,Fearless,Chapter8 9,Help III,Chapter9")
                .SetString(ConfigKey.ChapterProduction,
                    "Id,ChapterName,ChapterIconAddress 1,Love I,Chapter1 2,Help I,Chapter2 3,Family,Chapter3 4,Discovery,Chapter4 5,Love II,Chapter5 6,Animals,Chapter6 7,Help II,Chapter7 8,Fearless,Chapter8 9,Help III,Chapter9")
                .SetString(ConfigKey.ClientVersion, Application.version)
                .SetFloat(ConfigKey.AdCooldown, 60f)
                .SetString(ConfigKey.RateLevel, "10,35,65")
                .SetString(ConfigKey.HintLevel, "1,5")
                .SetBool(ConfigKey.IsShowAOA, true)
                .SetBool(ConfigKey.RequireInternet, true)
                .SetFloat(ConfigKey.InternetFailedDelay, 7f)
                .SetString(ConfigKey.ShowAdLevels, "10")
                .SetBool(ConfigKey.ResumeAds, true)
                .SetBool(ConfigKey.RatingPopup, true)
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
                .SetInt(ConfigKey.HardLevelAdditionalTime, 30)
                .SetInt(ConfigKey.HardLevelWarningTime, 10)
                .SetInt(ConfigKey.CooldownInterAfterShowReward, 30)
                .SetInt(ConfigKey.LifeCooldown, 900)
                .SetInt(ConfigKey.MaxLife, 10)
                .SetInt(ConfigKey.LifeRecoverAfterChapter, 2);

            await RemoteConfig.SetDefaultValues(blueprint);

            this.RemoteConfig.OnFetchSuccess += () =>
            {
                this.IsRemoteConfigInitialized = true;
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
                Debug.LogError($"[RemoteConfig] Fetching Error: " + configFetchError);
            };

            var timeOutSeconds = 2f;
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

        private void HandleFirstAudio()
        {
            this.AudioService.PlaySound(this.BGMSoundKey);
            this.AudioService.SetMusicVolPercentage(this.GameData.SettingModel.MusicOn ? 1 : 0);
            this.AudioService.SetSoundVolPercentage(this.GameData.SettingModel.SoundOn ? 1 : 0);

            // Debug.Log($"--- (Audio) MusicOn: {this.GameData.SettingModel.MusicOn} --- {this.AudioService.MusicVolPercentage}");
            // Debug.Log($"--- (Audio) SoundOn: {this.GameData.SettingModel.SoundOn} --- {this.AudioService.SoundVolPercentage}");
        }
    }
}