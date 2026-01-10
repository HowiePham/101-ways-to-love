using System.Threading;
using _Modules._UI.LoseView.Scripts;
using _Modules.Gameflow_Events_.Scripts;
using Cysharp.Threading.Tasks;
using GameScenes;
using LeveLoaders;
using Mimi.Events;
using Mimi.Games;
using Mimi.Prototypes;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using UnityEngine;

namespace Mimi
{
    public class PlayingState : BaseSceneState
    {
        private LevelInfo currentLevel;
        private LevelInfo nextLevel;
        private GameObject levelRoot;
        private LevelPlayer levelPlayer;
        private ILevelLoader levelLoader;
        private readonly DisposableBag eventBag = new DisposableBag();
        private const float TimeTest = 120f;

        public override void OnInitialized()
        {
            this.levelLoader = new AddressableLevelLoader(Context.LevelRepository);
        }

        public override void Enter()
        {
            base.Enter();
            Context.EventSubscriber.Subscribe<StageCompleted>(StageCompletedHandler).AddToBag(this.eventBag);
            Context.EventSubscriber.Subscribe<StageFailed>(StageFailedHandler).AddToBag(this.eventBag);
            Context.EventSubscriber.Subscribe<NextLevelClicked>(NextLevelHandler).AddToBag(this.eventBag);
            Context.EventSubscriber.Subscribe<LevelTryAgain>(TryAgainLevelHandler).AddToBag(this.eventBag);
            Context.EventSubscriber.Subscribe<SelectLevel>(SelectLevelHandler).AddToBag(this.eventBag);
            PlayLevel(Context.RuntimeState.CurrentLevelOrder.Value);
        }

        private async UniTask SelectLevelHandler(SelectLevel selectLevel, CancellationToken cancellationToken)
        {
            ServiceLocator.Global.Get<IPoolService>().Despawn(this.levelRoot);
            PlayLevel(selectLevel.LevelOrder);
            await UniTask.CompletedTask;
        }

        private async UniTask TryAgainLevelHandler(LevelTryAgain levelTryAgain, CancellationToken cancellationToken)
        {
            ServiceLocator.Global.Get<IPoolService>().Despawn(this.levelRoot);
            PlayLevel(Context.RuntimeState.CurrentLevelOrder.Value);
            await UniTask.CompletedTask;
        }

        private async UniTask NextLevelHandler(NextLevelClicked nextLevelClicked, CancellationToken cancellationToken)
        {
            ServiceLocator.Global.Get<IPoolService>().Despawn(this.levelRoot);

            if (!IsLastLevel())
            {
                int oldValue = this.Context.RuntimeState.CurrentLevelOrder.Value;
                this.Context.RuntimeState.CurrentLevelOrder.Set(oldValue + 1);
            }

            Context.SaveManager.Save();
            PlayLevel(this.Context.RuntimeState.CurrentLevelOrder.Value);
            await UniTask.CompletedTask;
        }

        private bool IsLastLevel()
        {
            return Context.LevelOrder.IsLast(this.currentLevel.Id);
        }

        private async UniTask StageFailedHandler(StageFailed stageFailed, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
        }

        private async UniTask StageCompletedHandler(StageCompleted stageCompleted, CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
        }

        public override void Exit()
        {
            base.Exit();
            this.eventBag.Dispose();
        }

        private async void PlayLevel(int levelOrder)
        {
            Debug.Log($"--- (LEVEL) Playing level : {levelOrder}");
            this.currentLevel = Context.LevelOrder.GetByOrder(levelOrder);
            this.nextLevel = Context.LevelOrder.GetNextLevel(levelOrder);
            await PlayLevel(this.currentLevel);
        }

        private async UniTask PlayLevel(LevelInfo currentLevelInfo)
        {
            GameObject levelPrefab = await this.levelLoader.Load(currentLevelInfo.Id);
            this.levelRoot = ServiceLocator.Global.Get<IPoolService>().Spawn(levelPrefab);
            // this.levelRoot.transform.SetPosition(0f, 0.5f, 0f);
            this.levelPlayer = this.levelRoot.GetComponent<LevelPlayer>();
            await UniTask.Delay(500);
            await Context.EventPublisher.PublishAsync(new LevelStarted(currentLevelInfo.Id));
            await this.levelPlayer.Play();

            ShowGameplayView();
        }

        private void ShowGameplayView()
        {
            var gameplayViewPresenter = this.Presenter.GetViewPresenter<GameplayViewPresenter>();
            gameplayViewPresenter.Show();
        }
    }
}