using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using DG.Tweening;
using Mimi.Analytics.Tracking.Trackers;
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
            await UniTask.WaitUntil(() => this.gameContext.IsInitialized);
            Debug.Log($"--- (BOOT) Core Service initialized at {this.totalLoadingStopwatch.ElapsedMilliseconds}ms");
            this.earlyProgressTween?.Kill();

            this.gameContext.CreateServices();

            UniTask fakeLoadingBarProgress = DOTween.To(() => this.loadingPercentage,
                value =>
                {
                    this.loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(this.loadingPercentage);
                }, 0.8f, 0.3f).AsyncWaitForCompletion().AsUniTask();

            UniTask waitForServiceInitialized = UniTask.WaitUntil(() => this.gameContext.IsServiceInitialized);

            var sceneLoadStart = this.totalLoadingStopwatch.ElapsedMilliseconds;
            UniTask sceneLoadTask = this.gameContext.LoadSceneAsync(this.nextSceneType.Type);

            await UniTask.WhenAll(fakeLoadingBarProgress, waitForServiceInitialized, sceneLoadTask);
            Debug.Log($"--- (BOOT) Services + scene ready in {this.totalLoadingStopwatch.ElapsedMilliseconds - sceneLoadStart}ms (total {this.totalLoadingStopwatch.ElapsedMilliseconds}ms)");

            this.gameContext.AnalyticTracker.LogEvent(new Feature_LOADING_FINISH()
            {
                eventName = Feature_LOADING_FINISH.EVENT_NAME.loading_finish,
                placement = "app_open",
                is_load = "1",
                load_time = this.totalLoadingStopwatch.ElapsedMilliseconds.ToString()
            });

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