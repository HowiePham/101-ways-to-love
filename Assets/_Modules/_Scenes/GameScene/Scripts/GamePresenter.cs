using _Modules._UI.CheatView.Scripts;
using _Modules._UI.LoseView.Scripts;
using _Modules._UI.TransitionView.Scripts;
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

            // var selectLevelViewPresenter = new SelectLevelPresenter(this, this.Transform, gameContext.LevelRepository,
            //     gameContext.AudioService, gameContext.Ads, gameContext.RuntimeState, gameContext.EventPublisher);
            // AddViewPresenter(selectLevelViewPresenter);
            var chapterSelectLevelViewPresenter = new ChapterSelectLevelPresenter(this, this.Transform, gameContext.ChapterLevelRepo,
                gameContext.LevelOrder, gameContext.AudioService, gameContext.Ads, gameContext.RuntimeState, gameContext.EventPublisher,
                gameContext.EventSubscriber, gameContext.LifeSystem);
            AddViewPresenter(chapterSelectLevelViewPresenter);
            Debug.Log($"--- (PRESENTER) Init GameplayViewPresenter");
            var gameplayViewPresenter = new GameplayViewPresenter(this, this.Transform, gameContext.EventPublisher, gameContext.EventSubscriber,
                gameContext.RuntimeState, gameContext.LifeSystem, gameContext.HintLevelConfig, gameContext.Ads, gameContext.DialogManager,
                gameContext.ChapterLevelRepo, gameContext.LevelOrder);
            AddViewPresenter(gameplayViewPresenter);
            var hardLevelViewPresenter = new HardLevelViewPresenter(this, this.Transform, gameContext.RemoteConfig, gameContext.Ads, gameContext.DialogManager,
                gameContext.EventPublisher, gameContext.RuntimeState);
            AddViewPresenter(hardLevelViewPresenter);
            Debug.Log($"--- (PRESENTER) Init SettingViewPresenter");
            var settingViewPresenter = new SettingViewPresenter(this, this.Transform,
                gameContext.EventPublisher, gameContext.GameData.SettingModel, gameContext.SaveManager,
                gameContext.RuntimeState, gameContext.AudioService, gameContext.Ads);
            AddViewPresenter(settingViewPresenter);
            Debug.Log($"--- (PRESENTER) Init WinViewPresenter");
            var winViewPresenter = new WinViewPresenter(this, this.Transform, gameContext.EventPublisher,
                gameContext.RuntimeState, gameContext.Ads, gameContext.ShowInterstitialLevelConfig, gameContext.GameData, gameContext.LevelOrder,
                gameContext.RemoteConfig, gameContext.LifeSystem);
            AddViewPresenter(winViewPresenter);
            Debug.Log($"--- (PRESENTER) Init LoseViewPresenter");
            var loseViewPresenter = new LoseViewPresenter(this, this.Transform, gameContext.EventPublisher);
            AddViewPresenter(loseViewPresenter);
            var chapterUnlockPresenter = new ChapterUnlockPresenter(
                this, this.Transform, gameContext.EventPublisher, gameContext.ChapterLevelRepo, gameContext.LevelOrder,
                gameContext.RuntimeState, gameContext.RemoteConfig, gameContext.DialogManager);
            AddViewPresenter(chapterUnlockPresenter);
            var removeAdsViewPresenter = new RemoveAdsViewPresenter(this, this.Transform, gameContext.EventPublisher, gameContext.DialogManager, gameContext.InAppPurchaseStore);
            AddViewPresenter(removeAdsViewPresenter);
            var transitionViewPresenter = new TransitionViewPresenter(this, this.Transform);
            AddViewPresenter(transitionViewPresenter);

#if DEVELOPMENT
            var cheatViewPresenter = new CheatViewPresenter(this, this.Transform, gameContext.EventPublisher);
            AddViewPresenter(cheatViewPresenter);
#endif
        }
    }
}