using _Modules._UI.CheatView.Scripts;
using _Modules._UI.LoseView.Scripts;
using _Modules._UI.WinView.Scripts;
using Mimi.Prototypes.UI;
using UnityEngine;

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

            Debug.Log($"--- (PRESENTER) Init GameplayViewPresenter");
            var gameplayViewPresenter = new GameplayViewPresenter(this, this.Transform, gameContext.EventPublisher, gameContext.EventSubscriber,
                gameContext.RuntimeState, gameContext.LifeSystem, gameContext.HintLevelConfig, gameContext.Ads, gameContext.DialogManager);
            AddViewPresenter(gameplayViewPresenter);
            var hardLevelViewPresenter = new HardLevelViewPresenter(this, this.Transform, gameContext.RemoteConfig, gameContext.Ads, gameContext.DialogManager,
                gameContext.EventPublisher);
            AddViewPresenter(hardLevelViewPresenter);
            Debug.Log($"--- (PRESENTER) Init SettingViewPresenter");
            var settingViewPresenter = new SettingViewPresenter(this, this.Transform,
                gameContext.EventPublisher, gameContext.GameData.SettingModel, gameContext.SaveManager, gameContext.RuntimeState, gameContext.AudioService);
            AddViewPresenter(settingViewPresenter);
            Debug.Log($"--- (PRESENTER) Init WinViewPresenter");
            var winViewPresenter = new WinViewPresenter(this, this.Transform, gameContext.EventPublisher, gameContext.RuntimeState);
            AddViewPresenter(winViewPresenter);
            Debug.Log($"--- (PRESENTER) Init LoseViewPresenter");
            var loseViewPresenter = new LoseViewPresenter(this, this.Transform, gameContext.EventPublisher);
            AddViewPresenter(loseViewPresenter);
            var removeAdsViewPresenter = new RemoveAdsViewPresenter(this, this.Transform);
            AddViewPresenter(removeAdsViewPresenter);

#if DEVELOPMENT
            var cheatViewPresenter = new CheatViewPresenter(this, this.Transform, gameContext.EventPublisher);
            AddViewPresenter(cheatViewPresenter);
#endif
        }
    }
}