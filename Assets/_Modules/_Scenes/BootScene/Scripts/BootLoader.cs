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
        [SerializeField] private float fakeLoadingSecs = 15f;

        [SerializeField, ValueDropdown("GetSoundGroups")]
        private string backgroundMusic;

        [SerializeField, ClassExtends(typeof(BaseSceneController))]
        private ClassTypeReference nextSceneType;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public async UniTask StartLoading()
        {
            this.bootView.Show();
            await this.bootView.RunLogoEffect();
            await this.bootView.ShowLoadingBarEffect();
            await Load();
            this.gameContext.EventPublisher.PublishAsync(new BootGameCompleted());
            this.bootView.Hide();
            Destroy(gameObject);
        }

        private async UniTask Load()
        {
            float loadingSecs = Application.isEditor ? 1f : fakeLoadingSecs;
            float loadingPercentage = 0f;

            UniTask loadConfigTask = DOTween.To(() => loadingPercentage,
                value =>
                {
                    loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(loadingPercentage);
                }, 0.1f, 2f).AsyncWaitForCompletion().AsUniTask();

            UniTask fetchConfigTask = UniTask.WhenAll(loadConfigTask);
            await fetchConfigTask;
            await UniTask.WaitUntil(() => this.gameContext.IsRemoteConfigInitialized);

            this.gameContext.CreateServices();
            UniTask fakeLoadingBarProgress = DOTween.To(() => loadingPercentage,
                value =>
                {
                    loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(loadingPercentage);
                }, 0.8f, loadingSecs).AsyncWaitForCompletion().AsUniTask();

            UniTask waitForContextInitialized = UniTask.WaitUntil(() => this.gameContext.IsInitialized);
            UniTask loadingProgress =
                UniTask.WhenAll(fakeLoadingBarProgress, waitForContextInitialized);

            await loadingProgress;
            UniTask loadNextScene = this.gameContext.LoadSceneAsync(this.nextSceneType.Type);
            await loadNextScene;

            await DOTween.To(() => loadingPercentage,
                value =>
                {
                    loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(loadingPercentage);
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