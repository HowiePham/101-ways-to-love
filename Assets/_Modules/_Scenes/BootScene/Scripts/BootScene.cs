using Cysharp.Threading.Tasks;
using Mimi.Prototypes.SceneManagement;
using UnityEngine;

namespace Mimi.Prototypes
{
    public class BootScene : BaseSceneController<BaseGameContext>
    {
        [SerializeField] private BootLoader bootLoader;

        public override void RequestAssets()
        {
        }

        protected override async UniTask OnEnter()
        {
            await base.OnEnter();
            this.bootLoader.StartLoading();
        }
    }
}