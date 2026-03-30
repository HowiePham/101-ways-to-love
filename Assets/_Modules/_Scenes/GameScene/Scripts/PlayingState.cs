using System.Threading;
using _Modules._UI.LoseView.Scripts;
using _Modules.GameEvent.Scripts;
using _Modules.Gameflow_Events_.Scripts;
using Cysharp.Threading.Tasks;
using FrogunnerGames;
using Games;
using GameScenes;
using Lean.Touch;
using LeveLoaders;
using Mimi.Audio;
using Mimi.Events;
using Mimi.Games;
using Mimi.Games.Events;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using Mimi.VisualActions.Audio;
using UnityEngine;

namespace Mimi
{
    public class PlayingState : BaseSceneState
    {
        [SerializeField, SoundKey] private string interactingSoundKey;
        [SerializeField, SoundKey] private string bgmSoundKey;
        [SerializeField] private GameObject winCamera;

        private string[] levelGeneralSoundKeys;
        private LevelInfo currentLevel;
        private HintPlayer hintPlayer;
        private LevelInfo nextLevel;
        private GameObject levelRoot;
        private LevelPlayer levelPlayer;
        private ILevelLoader levelLoader;
        private readonly DisposableBag eventBag = new DisposableBag();
        private HardLevelViewPresenter HardLevelViewPresenter => Presenter.GetViewPresenter<HardLevelViewPresenter>();

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
            Context.EventSubscriber.Subscribe<SkipLevel>(SkipLevelHandler).AddToBag(this.eventBag);
            Context.EventSubscriber.Subscribe<LevelTryAgain>(TryAgainLevelHandler).AddToBag(this.eventBag);
            Context.EventSubscriber.Subscribe<SelectLevel>(SelectLevelHandler).AddToBag(this.eventBag);
            Context.EventSubscriber.Subscribe<BackHome>(BackHomeHandler).AddToBag(this.eventBag);
            Context.EventSubscriber.Subscribe<DestroyLevelRequested>(DestroyLevelRequestedHandler).AddToBag(this.eventBag);
            Context.EventSubscriber.Subscribe<UseHint>(UseHintHandler).AddToBag(this.eventBag);

            HardLevelViewPresenter.GetView<HardLevelView>().OnClickPlay +=
                LimitedTimeViewClickPlayHandler;

            Messenger.AddListener(EventKey.LevelWin, LevelWinHandler);

            LeanTouch.OnFingerDown += ClickSoundHandler;

            Context.LifeSystem.RunTimer();
            // PlayLevel(Context.RuntimeState.CurrentLevelOrder.Value);
            var chapterSelectLevelPresenter = this.Presenter.GetViewPresenter<ChapterSelectLevelPresenter>();
            chapterSelectLevelPresenter.Show();
        }

        private void LimitedTimeViewClickPlayHandler()
        {
            this.levelPlayer.Play();
            HardLevelViewPresenter.StartTimer();
        }

        private async UniTask SkipLevelHandler(SkipLevel skipLevel, CancellationToken cancellation)
        {
            this.winCamera.SetActive(true);
            await this.levelPlayer.EndLevel();
            Messenger.Broadcast(EventKey.LevelWin);
        }

        private async UniTask UseHintHandler(UseHint useHint, CancellationToken cancellationToken)
        {
            this.hintPlayer.ShowNextHint();
            await UniTask.CompletedTask;
        }

        private void LevelWinHandler()
        {
            this.winCamera.SetActive(true);
            int currentLevelOrder = this.Context.RuntimeState.CurrentLevelOrder.Value;
            this.Context.EventPublisher.PublishAsync(new LevelCompleted(currentLevelOrder.ToString(), LevelCompletionStatus.Win));

            if (!IsLastLevel())
            {
                int nextLevelOrder = currentLevelOrder + 1;
                int topLevel = this.Context.RuntimeState.TopLevelOrder.Value;
                if (nextLevelOrder > topLevel)
                {
                    this.Context.RuntimeState.TopLevelOrder.Set(nextLevelOrder);
                }
            }

            Context.SaveManager.Save();
        }

        private void ClickSoundHandler(LeanFinger finger)
        {
            this.Context.AudioService.PlaySound(this.interactingSoundKey);
        }

        private async UniTask SelectLevelHandler(SelectLevel selectLevel, CancellationToken cancellationToken)
        {
            if (this.levelPlayer != null)
            {
                DestroyOldLevelRoot();
                var gameplayViewPresenter = this.Presenter.GetViewPresenter<GameplayViewPresenter>();
                gameplayViewPresenter.Hide();
            }

            this.Context.RuntimeState.CurrentLevelOrder.Set(selectLevel.LevelOrder);
            PlayLevel(selectLevel.LevelOrder);
            await UniTask.CompletedTask;
        }

        private async UniTask BackHomeHandler(BackHome backHome, CancellationToken cancellationToken)
        {
            DestroyOldLevelRoot();
            var gameplayViewPresenter = this.Presenter.GetViewPresenter<GameplayViewPresenter>();
            gameplayViewPresenter.Hide();
            var chapterSelectLevelPresenter = this.Presenter.GetViewPresenter<ChapterSelectLevelPresenter>();
            chapterSelectLevelPresenter.Show();
            await UniTask.CompletedTask;
        }

        private async UniTask TryAgainLevelHandler(LevelTryAgain levelTryAgain, CancellationToken cancellationToken)
        {
            DestroyOldLevelRoot();

            PlayLevel(Context.RuntimeState.CurrentLevelOrder.Value);
            await UniTask.CompletedTask;
        }

        private async UniTask DestroyLevelRequestedHandler(DestroyLevelRequested destroyLevelRequested, CancellationToken cancellationToken)
        {
            DestroyOldLevelRoot();
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
                int newValue = oldValue + 1;
                this.Context.RuntimeState.CurrentLevelOrder.Set(newValue);
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
            this.currentLevel = Context.LevelOrder.GetByOrder(levelOrder);
            this.nextLevel = Context.LevelOrder.GetNextLevel(levelOrder);
            Debug.Log($"--- (LEVEL) Playing level : {levelOrder + 1} --- PrefabAddress: {this.currentLevel.PrefabAddress}");
            await PlayLevel(this.currentLevel, levelOrder);
        }

        private async UniTask PlayLevel(LevelInfo currentLevelInfo, int levelOrder)
        {
            this.winCamera.SetActive(false);
            GameObject levelPrefab = await this.levelLoader.Load(currentLevelInfo.Id);
            this.levelRoot = ServiceLocator.Global.Get<IPoolService>().Spawn(levelPrefab);
            this.hintPlayer = this.levelRoot.GetComponent<HintPlayer>();
            this.levelPlayer = this.levelRoot.GetComponent<LevelPlayer>();
            TurnOffOldLevelGeneralSound();
            UpdateLevelGeneralSound();
            ShowGameplayView();
            this.Context.StopSound(this.bgmSoundKey);

            await this.hintPlayer.Init();
            this.hintPlayer.SetLevelTutorial(CanShowTutorial(levelOrder + 1));

            await Context.EventPublisher.PublishAsync(new LevelStarted(currentLevelInfo.Id));

            if (Context.HardLevelConfig.HasLevel(StringNumber.IntToText(levelOrder + 1)))
            {
                HardLevelViewPresenter.SetClockStartTime(Context.RemoteConfig.GetValue(ConfigKey.HardLevelBaseTime).Int);
                HardLevelViewPresenter.Show();
            }
            else
            {
                this.levelPlayer.Play();
            }
        }

        private bool CanShowTutorial(int currentLevelOrder)
        {
            return this.Context.HintLevelConfig.HasLevel(currentLevelOrder.ToString());
        }

        private void ShowGameplayView()
        {
            Debug.Log($"--- (PRESENTER) Showing Gameplay View");
            var gameplayViewPresenter = this.Presenter.GetViewPresenter<GameplayViewPresenter>();
            gameplayViewPresenter.Show();
            gameplayViewPresenter.InitStepPoint(this.hintPlayer.TotalStep);
        }

        private void UpdateLevelGeneralSound()
        {
            PlayAudio[] levelPlayerLevelGeneralAudio = this.levelPlayer.LevelGeneralAudio;
            this.levelGeneralSoundKeys = new string[levelPlayerLevelGeneralAudio.Length];

            Debug.Log($"--- (GAME) Update Level {this.currentLevel.Id} Sound: {levelPlayerLevelGeneralAudio.Length}");

            for (int i = 0; i < levelPlayerLevelGeneralAudio.Length; i++)
            {
                var audio = levelPlayerLevelGeneralAudio[i];
                this.levelGeneralSoundKeys[i] = audio.SoundKey;
            }
        }

        private void TurnOffOldLevelGeneralSound()
        {
            if (this.levelGeneralSoundKeys == null || this.levelGeneralSoundKeys.Length <= 0)
            {
                return;
            }

            foreach (string soundKey in this.levelGeneralSoundKeys)
            {
                if (string.IsNullOrEmpty(soundKey))
                {
                    continue;
                }

                this.Context.StopSound(soundKey);
            }
        }
    }
}