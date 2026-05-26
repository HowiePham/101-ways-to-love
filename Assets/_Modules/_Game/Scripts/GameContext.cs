using System.Collections.Generic;
using System.Linq;
using Ads;
using Economy.Resources;
using Games;
using Mimi.Audio;
using Mimi.Games.InitSteps;
using Mimi.Games.Plugins;
using Mimi.Games.ProjectConfigs;
using Mimi.Loot.Currencies;
using Mimi.Loots;
using Mimi.Prototypes.LevelManagement;
using Mimi.ServiceLocators;
using Sirenix.OdinInspector;
using Tracking;
using UnityEngine;

namespace Mimi.Prototypes
{
    public class GameContext : BaseGameContext
    {
        public ILevelRepository LevelRepository { private set; get; }
        public ILevelOrder LevelOrder { private set; get; }
        public ChapterLevelRepository ChapterLevelRepo { private set; get; }
        public AngelSkinRepo AngelSkinRepo { private set; get; }
        public LevelConfig HintLevelConfig { private set; get; }
        public LevelConfig HardLevelConfig { private set; get; }

        private CompositeLootProcessor lootProcessor;
        private CompositeLootFactory lootFactory;

        public override void CreateServices()
        {
            CreateLevelServices();
            InitAngelSkinServices();
            InitShowInterstitialLevelConfig();
            InitHintLevelConfig();
            InitHardLevelConfig();
            InitRateLevelConfig();
            InitLootSystem();

            this.IsServiceInitialized = true;
        }

        private void InitRateLevelConfig()
        {
            RateConfig.ParseConfig(this.RemoteConfig.GetValue(ConfigKey.RateLevel).String);
        }

        private void InitHardLevelConfig()
        {
            this.HardLevelConfig = new LevelConfig();
            this.HardLevelConfig.ParseConfig(this.RemoteConfig.GetValue(ConfigKey.HardLevel).String);
            LogInitializeEvent("init_hard_level_config");
        }

        protected override void AddInitSteps(IGameInitiator gameInitiator, ProjectConfig projectConfig)
        {
        }

        protected override void AddGlobalPlugins(CompositePlugin pluginInstaller)
        {
            pluginInstaller.AddPlugin(new LoadFirstAdsPlugin(this));
            pluginInstaller.AddPlugin(new ShowOpenAdAfterBootPlugin(this.Ads, this.EventSubscriber, this.RemoteConfig, this.IsFirstSession));
            pluginInstaller.AddPlugin(new ShowOpenAdOnResumePlugin(this.Ads, this.EventSubscriber, this.IsRemoveAds, this.RemoteConfig, this.IsFirstSession));
            pluginInstaller.AddPlugin(new LogHintLevelShow(this.RuntimeState, this.EventSubscriber, this.AnalyticTracker));
            pluginInstaller.AddPlugin(new LogLevelStartPlugin(this.RuntimeState, this.EventSubscriber, this.AnalyticTracker, this.LifeSystem));
            pluginInstaller.AddPlugin(new LogLevelCompletedPlugin(this.RuntimeState, this.EventSubscriber, this.AnalyticTracker, this.Ads, this.LifeSystem));
            pluginInstaller.AddPlugin(new LogLevelSkipPlugin(this.RuntimeState, this.EventSubscriber, this.AnalyticTracker));
            pluginInstaller.AddPlugin(new LogAdClickPlugin(this.Ads, this.AnalyticTracker));
            pluginInstaller.AddPlugin(new LogAdCompletePlugin(this.Ads, this.AnalyticTracker));
            pluginInstaller.AddPlugin(new LogAdRequestPlugin(this.Ads, this.AnalyticTracker));
            pluginInstaller.AddPlugin(new LogLevelExitPlugin(this.RuntimeState, this.EventSubscriber, this.AnalyticTracker, this.Ads, this.LifeSystem));
            pluginInstaller.AddPlugin(new LogLevelReopenPlugin(this.RuntimeState, this.EventSubscriber, this.AnalyticTracker));
            pluginInstaller.AddPlugin(new LogResourceChangedPlugin((ResourceCollection)this.PlayerResources, this.AnalyticTracker));
            pluginInstaller.AddPlugin(new LogIapPlugin(this.EventSubscriber, this.AnalyticTracker, this.InAppPurchaseStore));
            // pluginInstaller.AddPlugin(new LogSessionDurationPlugin(this.RuntimeState, this.EventSubscriber, this.AnalyticTracker, this.LifeSystem, this.Ads));
            pluginInstaller.AddPlugin(new RemoveAdOnPurchasePlugin(this));
            pluginInstaller.AddPlugin(new DelayInterAfterShowReward(this));
        }

        private void InitAngelSkinServices()
        {
            AngelSkinRepo = new AngelSkinRepo(GetDataSheet<SheetAngelSkinModel>("AngelSkinRepo"));

            foreach (SheetAngelSkinModel model in AngelSkinRepo.GetAll())
            {
                if (!this.GameData.AngelSkins.ContainsKey(model.Id))
                {
                    this.GameData.AngelSkins[model.Id] = false;
                }
            }
        }

        private void CreateLevelServices()
        {
            LevelRepository = new SheetLevelRepository(GetDataSheet<SheetLevelModel>());
            LevelOrder = new LinearLevelOrder(LevelRepository, GetLevelOrderEntries());
            ChapterLevelRepo = new ChapterLevelRepository(LevelOrder, GetChapterModels());

            LogInitializeEvent("init_level_service");
        }

        private IEnumerable<LevelOrderEntry> GetLevelOrderEntries()
        {
            string levelConfigString = string.Empty;
#if DEVELOPMENT
            levelConfigString = this.RemoteConfig.GetValue(ConfigKey.LevelDevelopment).String;
#else
            levelConfigString = this.RemoteConfig.GetValue(ConfigKey.LevelProduction).String;
#endif
            if (!string.IsNullOrEmpty(levelConfigString))
            {
                var remoteEntries = new RemoteLinearOrderParser(levelConfigString).Parse().ToList();
                if (remoteEntries.Count > 0)
                {
                    return remoteEntries;
                }
            }

            var sheetName = "LevelProd";
#if DEVELOPMENT
            sheetName = "LevelDev";
#else
            sheetName = "LevelProd";
#endif

            return GetDataSheet<SheetOrderModel>(sheetName)
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .Select(x => new LevelOrderEntry(x.Id, int.TryParse(x.Chapter, out int ch) ? ch : 1));
        }

        private IEnumerable<IChapterModel> GetChapterModels()
        {
            string chapterConfigString = string.Empty;
#if DEVELOPMENT
            chapterConfigString = this.RemoteConfig.GetValue(ConfigKey.ChapterDevelopment).String;
#else
            chapterConfigString = this.RemoteConfig.GetValue(ConfigKey.ChapterProduction).String;
#endif
            if (!string.IsNullOrEmpty(chapterConfigString))
            {
                var remoteChapters = new RemoteChapterParser(chapterConfigString).Parse();
                if (remoteChapters.Length > 0)
                {
                    return remoteChapters;
                }
            }

            var sheetName = "ChapterProd";
#if DEVELOPMENT
            sheetName = "ChapterDev";
#else
            sheetName = "ChapterProd";
#endif
            return GetDataSheet<SheetChapterModel>(sheetName);
        }

        private void InitHintLevelConfig()
        {
            this.HintLevelConfig = new LevelConfig();
            this.HintLevelConfig.ParseConfig(this.RemoteConfig.GetValue(ConfigKey.HintLevel).String);
            LogInitializeEvent("init_hint_level_config");
        }

        private void InitShowInterstitialLevelConfig()
        {
            this.ShowInterstitialLevelConfig = new LevelConfig();
            this.ShowInterstitialLevelConfig.ParseConfig(this.RemoteConfig.GetValue(ConfigKey.ShowAdLevels).String);
            LogInitializeEvent("init_show_interstitial_level_config");
        }

        private void InitLootSystem()
        {
            this.lootFactory = new CompositeLootFactory();
            this.lootFactory.AddFactory("Currency", new CurrencyLootFactory());

            this.lootProcessor = new CompositeLootProcessor();
            this.lootProcessor.AddProcessor("Currency", new CurrencyLootProcessor(PlayerResources));
            this.lootProcessor.AddProcessor("Skin", new SkinLootProcessor(this.GameData));

            this.lootFactory.AddFactory("Skin", new SkinLootFactory());
            LogInitializeEvent("init_loot_system");
        }

        [Button]
        private void TestLife()
        {
            this.EventPublisher.PublishAsync(new LifeUsing("test_life"));
        }
    }
}