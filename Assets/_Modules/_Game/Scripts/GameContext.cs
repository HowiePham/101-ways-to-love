using System.Linq;
using Games;
using Mimi.Games.InitSteps;
using Mimi.Games.Plugins;
using Mimi.Games.ProjectConfigs;
using Mimi.Prototypes.LevelManagement;
using Sirenix.OdinInspector;

namespace Mimi.Prototypes
{
    public class GameContext : BaseGameContext
    {
        public ILevelRepository LevelRepository { private set; get; }
        public ILevelOrder LevelOrder { private set; get; }
        public LifeSystem LifeSystem { private set; get; }

        protected override void CreateServices()
        {
            CreateLevelServices();
            InitLifeSystem();
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

        private void InitLifeSystem()
        {
            LifeSystem = new LifeSystem(5, 30, this.EventPublisher, this.EventSubscriber);
        }
    }
}