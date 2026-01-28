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
using Mimi.VisualActions.Audio;
using UnityEngine;

namespace Mimi
{
    public class PlayingState : BaseSceneState
    {
        [SerializeField, SoundKey] private string interactingSoundKey;
        [SerializeField, SoundKey] private string bgmSoundKey;
        [SerializeField] private GameObject winCamera;
        [SerializeField] private int[] levelTutorials;

        private string[] levelGeneralSoundKeys;
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
            this.Context.AudioService.PlaySound(this.interactingSoundKey);
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
            if (CanShowTutorial(levelOrder + 1))
            {
                this.hintPlayer.ShowNextHint();
            }

            await UniTask.Delay(500);
            await Context.EventPublisher.PublishAsync(new LevelStarted(currentLevelInfo.Id));
            await this.levelPlayer.Play();
        }

        private bool CanShowTutorial(int currentLevelOrder)
        {
            foreach (int levelOrder in this.levelTutorials)
            {
                if (levelOrder == currentLevelOrder)
                {
                    return true;
                }
            }

            return false;
        }

        private void ShowGameplayView()
        {
            Debug.Log($"--- (PRESENTER) Showing Gameplay View");
            var gameplayViewPresenter = this.Presenter.GetViewPresenter<GameplayViewPresenter>();
            gameplayViewPresenter.Show();
            gameplayViewPresenter.InitStepPoint(this.hintPlayer.HintStepNumber);
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