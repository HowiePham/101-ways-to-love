using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using DG.Tweening;
using Mimi.Events;
using Mimi.Prototypes.SceneManagement;
using Sirenix.OdinInspector;
using TypeReferences;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mimi.Prototypes
{
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private BaseGameContext gameContext;
        [SerializeField] private BootView bootView;
        [SerializeField] private float fakeLoadingSecs;

        [SerializeField, ValueDropdown("GetSoundGroups")]
        private string backgroundMusic;

        [SerializeField, ClassExtends(typeof(BaseSceneController))]
        private ClassTypeReference nextSceneType;

        public static bool IsBootViewReady { get; private set; }

        private Tween earlyProgressTween;
        private float loadingPercentage;
        private readonly Stopwatch totalLoadingStopwatch = new Stopwatch();

        private void Awake()
        {
            DontDestroyOnLoad(this);
            IsBootViewReady = false;
            this.totalLoadingStopwatch.Restart();
            PrepareBootView().Forget();
        }

        private async UniTaskVoid PrepareBootView()
        {
            Application.targetFrameRate = 60;

            this.bootView.Show();
            await this.bootView.RunLogoEffect();
            await this.bootView.ShowLoadingBarEffect();

            this.loadingPercentage = 0f;
            await DOTween.To(() => this.loadingPercentage,
                value =>
                {
                    this.loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(this.loadingPercentage);
                }, 0.1f, 0.5f).AsyncWaitForCompletion();

            IsBootViewReady = true;

            float loadingSecs = this.fakeLoadingSecs;

            this.earlyProgressTween = DOTween.To(() => this.loadingPercentage,
                value =>
                {
                    this.loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(this.loadingPercentage);
                }, 0.7f, loadingSecs).SetEase(Ease.Linear);
        }

        public async UniTask StartLoading()
        {
            await Load();
            this.gameContext.EventPublisher.PublishAsync(new BootGameCompleted());
            this.bootView.Hide();
            this.totalLoadingStopwatch.Stop();
            Debug.Log($"--- (BOOT) Total game loading time: {this.totalLoadingStopwatch.ElapsedMilliseconds}ms");
            Destroy(gameObject);
        }

        private async UniTask Load()
        {
            await UniTask.WaitUntil(() => this.gameContext.IsRemoteConfigInitialized);
            Debug.Log($"--- (BOOT) RemoteConfig ready at {this.totalLoadingStopwatch.ElapsedMilliseconds}ms");

            this.gameContext.CreateServices();

            await this.earlyProgressTween.AsyncWaitForCompletion();

            UniTask fakeLoadingBarProgress = DOTween.To(() => this.loadingPercentage,
                value =>
                {
                    this.loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(this.loadingPercentage);
                }, 0.8f, 0.3f).AsyncWaitForCompletion().AsUniTask();

            UniTask waitForContextInitialized = UniTask.WaitUntil(() => this.gameContext.IsInitialized);
            await UniTask.WhenAll(fakeLoadingBarProgress, waitForContextInitialized);
            Debug.Log($"--- (BOOT) GameContext initialized at {this.totalLoadingStopwatch.ElapsedMilliseconds}ms");

            var sceneLoadStart = this.totalLoadingStopwatch.ElapsedMilliseconds;
            await this.gameContext.LoadSceneAsync(this.nextSceneType.Type);
            Debug.Log($"--- (BOOT) Next scene loaded in {this.totalLoadingStopwatch.ElapsedMilliseconds - sceneLoadStart}ms (total {this.totalLoadingStopwatch.ElapsedMilliseconds}ms)");

            await DOTween.To(() => this.loadingPercentage,
                value =>
                {
                    this.loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(this.loadingPercentage);
                }, 1f, 0.1f).AsyncWaitForCompletion().AsUniTask();
        }

#if UNITY_EDITOR
        private static IEnumerable<string> GetSoundGroups()
        {
            return MasterAudio.SafeInstance.GroupNames;
        }
#endif
    }
}