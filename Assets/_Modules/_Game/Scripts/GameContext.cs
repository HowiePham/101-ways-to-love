using System.Linq;
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
        private CompositeLootProcessor lootProcessor;
        private CompositeLootFactory lootFactory;

        protected override void CreateServices()
        {
            HandleFirstAudio();
            CreateLevelServices();
            // InitLifeSystem();
            InitLootSystem();
        }

        protected override void AddInitSteps(IGameInitiator gameInitiator, ProjectConfig projectConfig)
        {
        }

        protected override void AddGlobalPlugins(CompositePlugin pluginInstaller)
        {
        }

        private void HandleFirstAudio()
        {
            this.AudioService.PlaySound(this.bgmSoundKey);
            this.AudioService.SetMusicVolPercentage(this.GameData.SettingModel.MusicOn ? 1 : 0);
            this.AudioService.SetSoundVolPercentage(this.GameData.SettingModel.SoundOn ? 1 : 0);

            Debug.Log($"--- (Audio) MusicOn: {this.GameData.SettingModel.MusicOn} --- {this.AudioService.MusicVolPercentage}");
            Debug.Log($"--- (Audio) SoundOn: {this.GameData.SettingModel.SoundOn} --- {this.AudioService.SoundVolPercentage}");
        }

        private void CreateLevelServices()
        {
            LevelRepository = new SheetLevelRepository(GetDataSheet<SheetLevelModel>("LevelRepo"));
            var levelIdOrders = GetDataSheet<SheetOrderModel>().Select(x => x.Id).Distinct();
            LevelOrder = new LinearLevelOrder(LevelRepository, levelIdOrders);
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
            LifeSystem = new LifeSystem(5, 30, this.EventPublisher, this.EventSubscriber, this.DialogManager);
        }

        [Button]
        private void TestLife()
        {
            this.EventPublisher.PublishAsync(new LifeUsing());
        }
    }
}