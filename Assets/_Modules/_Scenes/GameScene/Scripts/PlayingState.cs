using System.Threading;
using _Modules._UI.LoseView.Scripts;
using _Modules.Gameflow_Events_.Scripts;
using Cysharp.Threading.Tasks;
using Games;
using GameScenes;
using Lean.Touch;
using LeveLoaders;
using Mimi.Audio;
using Mimi.Events;
using Mimi.Games;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using UnityEngine;

namespace Mimi
{
    public class PlayingState : BaseSceneState
    {
        [SerializeField, SoundKey] private string soundKey;
        [SerializeField] private GameObject winCamera;

        private LevelInfo currentLevel;
        private HintPlayer hintPlayer;
        private LevelInfo nextLevel;
        private GameObject levelRoot;
        private LevelPlayer levelPlayer;
        private ILevelLoader levelLoader;
        private readonly DisposableBag eventBag = new DisposableBag();

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
            Context.EventSubscriber.Subscribe<UseHint>(UseHintHandler).AddToBag(this.eventBag);

            Messenger.AddListener(EventKey.LevelWin, LevelWinHandler);

            LeanTouch.OnFingerDown += ClickSoundHandler;

            Context.LifeSystem.RunTimer();
            PlayLevel(Context.RuntimeState.CurrentLevelOrder.Value);
        }

        private async UniTask UseHintHandler(UseHint useHint, CancellationToken cancellationToken)
        {
            this.hintPlayer.ShowNextHint();
            await UniTask.CompletedTask;
        }

        private void LevelWinHandler()
        {
            this.winCamera.SetActive(true);
        }

        private void ClickSoundHandler(LeanFinger finger)
        {
            this.Context.AudioService.PlaySound(this.soundKey);
        }

        private async UniTask SelectLevelHandler(SelectLevel selectLevel, CancellationToken cancellationToken)
        {
            DestroyOldLevelRoot();
            PlayLevel(selectLevel.LevelOrder);
            await UniTask.CompletedTask;
        }

        private async UniTask TryAgainLevelHandler(LevelTryAgain levelTryAgain, CancellationToken cancellationToken)
        {
            DestroyOldLevelRoot();

            PlayLevel(Context.RuntimeState.CurrentLevelOrder.Value);
            await UniTask.CompletedTask;
        }

        private void DestroyOldLevelRoot()
        {
            this.levelPlayer.Cancel();
            ServiceLocator.Global.Get<IPoolService>().Despawn(this.levelRoot);
        }

        private async UniTask NextLevelHandler(NextLevelClicked nextLevelClicked, CancellationToken cancellationToken)
        {
            DestroyOldLevelRoot();

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
            LeanTouch.OnFingerDown -= ClickSoundHandler;

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
            this.winCamera.SetActive(false);
            GameObject levelPrefab = await this.levelLoader.Load(currentLevelInfo.Id);
            this.levelRoot = ServiceLocator.Global.Get<IPoolService>().Spawn(levelPrefab);
            this.hintPlayer = this.levelRoot.GetComponent<HintPlayer>();
            this.levelPlayer = this.levelRoot.GetComponent<LevelPlayer>();
            ShowGameplayView();

            await UniTask.Delay(500);
            await Context.EventPublisher.PublishAsync(new LevelStarted(currentLevelInfo.Id));
            await this.levelPlayer.Play();
        }

        private void ShowGameplayView()
        {
            Debug.Log($"--- (PRESENTER) Showing Gameplay View");
            var gameplayViewPresenter = this.Presenter.GetViewPresenter<GameplayViewPresenter>();
            gameplayViewPresenter.Show();
        }
    }
}