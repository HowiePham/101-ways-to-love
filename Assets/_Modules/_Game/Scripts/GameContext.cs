using System.Linq;
using Ads;
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
        [SerializeField, SoundKey] private string bgmSoundKey;
        public ILevelRepository LevelRepository { private set; get; }
        public ILevelOrder LevelOrder { private set; get; }
        public LifeSystem LifeSystem { private set; get; }
        public LevelConfig HintLevelConfig { private set; get; }
        public LevelConfig HardLevelConfig { private set; get; }
        private CompositeLootProcessor lootProcessor;
        private CompositeLootFactory lootFactory;

        public override void CreateServices()
        {
            HandleFirstAudio();
            CreateLevelServices();
            InitShowInterstitialLevelConfig();
            InitHintLevelConfig();
            InitHardLevelConfig();
            InitLootSystem();
            InitLifeSystem();
        }

        private void InitHardLevelConfig()
        {
            this.HardLevelConfig = new LevelConfig();
            this.HardLevelConfig.ParseConfig(this.RemoteConfig.GetValue(ConfigKey.HardLevel).String);
        }

        protected override void AddInitSteps(IGameInitiator gameInitiator, ProjectConfig projectConfig)
        {
        }

        protected override void AddGlobalPlugins(CompositePlugin pluginInstaller)
        {
            pluginInstaller.AddPlugin(new ShowOpenAdOnResumePlugin(this.Ads, this.EventSubscriber, this.IsRemoveAds, this.SessionRecorder, this.RemoteConfig));
            pluginInstaller.AddPlugin(new LoadFirstAdsPlugin(this));
        }

        private void HandleFirstAudio()
        {
            this.AudioService.PlaySound(this.bgmSoundKey);
            this.AudioService.SetMusicVolPercentage(this.GameData.SettingModel.MusicOn ? 1 : 0);
            this.AudioService.SetSoundVolPercentage(this.GameData.SettingModel.SoundOn ? 1 : 0);

            // Debug.Log($"--- (Audio) MusicOn: {this.GameData.SettingModel.MusicOn} --- {this.AudioService.MusicVolPercentage}");
            // Debug.Log($"--- (Audio) SoundOn: {this.GameData.SettingModel.SoundOn} --- {this.AudioService.SoundVolPercentage}");
        }

        private void CreateLevelServices()
        {
            LevelRepository = new SheetLevelRepository(GetDataSheet<SheetLevelModel>("LevelRepo"));
            var levelIdOrders = GetDataSheet<SheetOrderModel>().Select(x => x.Id).Distinct();
            LevelOrder = new LinearLevelOrder(LevelRepository, levelIdOrders);
        }

        private void InitHintLevelConfig()
        {
            this.HintLevelConfig = new LevelConfig();
            this.HintLevelConfig.ParseConfig(this.RemoteConfig.GetValue(ConfigKey.HintLevel).String);
        }

        private void InitShowInterstitialLevelConfig()
        {
            this.ShowInterstitialLevelConfig = new LevelConfig();
            this.ShowInterstitialLevelConfig.ParseConfig(this.RemoteConfig.GetValue(ConfigKey.ShowAdLevels).String);
        }

        private void InitLootSystem()
        {
            this.lootFactory = new CompositeLootFactory();
            this.lootFactory.AddFactory("Currency", new CurrencyLootFactory());

            this.lootProcessor = new CompositeLootProcessor();
            this.lootProcessor.AddProcessor("Currency", new CurrencyLootProcessor(PlayerResources));
        }

        private void InitLifeSystem()
        {
            LifeSystem = new LifeSystem(10, 30, this.EventPublisher, this.EventSubscriber, this.DialogManager, this.Ads);
        }

        [Button]
        private void TestLife()
        {
            this.EventPublisher.PublishAsync(new LifeUsing());
        }
    }
}