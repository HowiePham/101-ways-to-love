using System.Linq;
using Economy.Resources;
using Games;
using Mimi.Games.InitSteps;
using Mimi.Games.Plugins;
using Mimi.Games.ProjectConfigs;
using Mimi.Loot.Currencies;
using Mimi.Loots;
using Mimi.Prototypes.LevelManagement;
using Sirenix.OdinInspector;

namespace Mimi.Prototypes
{
    public class GameContext : BaseGameContext
    {
        public ILevelRepository LevelRepository { private set; get; }
        public ILevelOrder LevelOrder { private set; get; }
        public LifeSystem LifeSystem { private set; get; }
        public IResourceCollection ResourceCollection { private set; get; }
        private CompositeLootProcessor lootProcessor;
        private CompositeLootFactory lootFactory;

        protected override void CreateServices()
        {
            CreateLevelServices();
            // InitLifeSystem();
            InitResourceSystem();
            InitLootSystem();
        }

        protected override void AddInitSteps(IGameInitiator gameInitiator, ProjectConfig projectConfig)
        {
        }

        protected override void AddGlobalPlugins(CompositePlugin pluginInstaller)
        {
        }

        private void CreateLevelServices()
        {
            LevelRepository = new SheetLevelRepository(GetDataSheet<SheetLevelModel>("LevelRepo"));
            var levelIdOrders = GetDataSheet<SheetOrderModel>().Select(x => x.Id).Distinct();
            LevelOrder = new LinearLevelOrder(LevelRepository, levelIdOrders);
        }

        private void InitResourceSystem()
        {
            ResourceCollection = new ResourceCollection();
            IResource coinResource = new Resource("Coin");
            ResourceCollection.AddResource(coinResource);
        }

        private void InitLootSystem()
        {
            this.lootFactory = new CompositeLootFactory();
            this.lootFactory.AddFactory("Currency", new CurrencyLootFactory());

            this.lootProcessor = new CompositeLootProcessor();
            this.lootProcessor.AddProcessor("Currency", new CurrencyLootProcessor(ResourceCollection));
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