using Cysharp.Threading.Tasks;
using Mimi.Games.Plugins;

namespace Mimi.Prototypes
{
    public class GameScene : BaseGameScene
    {
        public override void RequestAssets()
        {
        }

        protected override void AddLocalPlugins(CompositePlugin pluginInstaller)
        {
        }

        protected override async UniTask OnEnter()
        {
            await base.OnEnter();
        }
    }
}