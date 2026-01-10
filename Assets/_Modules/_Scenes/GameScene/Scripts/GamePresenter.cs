using _Modules._UI.LoseView.Scripts;
using _Modules._UI.WinView.Scripts;
using Mimi.Prototypes.UI;

namespace Mimi.Prototypes
{
    public class GamePresenter : BaseScenePresenter<GameScene>
    {
        public override void Initialize(GameScene sceneController)
        {
            base.Initialize(sceneController);
            InitPresenters();
        }

        private void InitPresenters()
        {
            GameContext gameContext = this.SceneController.Context;

            var gameplayViewPresenter = new GameplayViewPresenter(this, this.Transform, gameContext.EventPublisher, gameContext.EventSubscriber);
            AddViewPresenter(gameplayViewPresenter);
            var settingViewPresenter = new SettingViewPresenter(this, this.Transform, gameContext.EventPublisher, gameContext.GameData.SettingModel, gameContext.SaveManager);
            AddViewPresenter(settingViewPresenter);
            var winViewPresenter = new WinViewPresenter(this, this.Transform, gameContext.EventPublisher, gameContext.RuntimeState);
            AddViewPresenter(winViewPresenter);
            var loseViewPresenter = new LoseViewPresenter(this, this.Transform, gameContext.EventPublisher);
            AddViewPresenter(loseViewPresenter);

#if DEVELOPMENT
            var cheatViewPresenter = new CheatViewPresenter(this, this.Transform, gameContext.EventPublisher);
            AddViewPresenter(cheatViewPresenter);
#endif
        }
    }
}