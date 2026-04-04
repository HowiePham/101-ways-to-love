using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using DG.Tweening;
using Mimi.Events;
using Mimi.Prototypes.SceneManagement;
using Sirenix.OdinInspector;
using TypeReferences;
using UnityEngine;

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

        private void Awake()
        {
            DontDestroyOnLoad(this);
            IsBootViewReady = false;
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

            float loadingSecs = Application.isEditor ? 1f : fakeLoadingSecs;
            // float loadingSecs = fakeLoadingSecs;
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
            Destroy(gameObject);
        }

        private async UniTask Load()
        {
            await UniTask.WaitUntil(() => this.gameContext.IsRemoteConfigInitialized);

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

            await this.gameContext.LoadSceneAsync(this.nextSceneType.Type);

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