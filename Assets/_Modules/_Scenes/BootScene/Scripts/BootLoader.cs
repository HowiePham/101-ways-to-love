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

        public async void StartLoading()
        {
            this.bootView.Show();
            await Load();
            this.gameContext.EventPublisher.PublishAsync(new BootGameCompleted());
            this.bootView.Hide();
            Destroy(gameObject);
        }

        private async UniTask Load()
        {
            float loadingSecs = Application.isEditor ? 1f : fakeLoadingSecs;
            float loadingPercentage = 0f;

            this.gameContext.CreateAudioService();
            this.gameContext.AudioService.PlaySound(this.backgroundMusic);

            UniTask fakeLoadingBarProgress = DOTween.To(() => loadingPercentage,
                value =>
                {
                    loadingPercentage = value;
                    this.bootView.SetLoadingPercentage(loadingPercentage);
                }, 0.9f, loadingSecs).AsyncWaitForCompletion().AsUniTask();

            UniTask waitForContextInitialized = UniTask.WaitUntil(() => this.gameContext.IsInitialized);
            UniTask loadNextScene = this.gameContext.LoadSceneAsync(this.nextSceneType.Type);
            UniTask loadingProgress =
                UniTask.WhenAll(fakeLoadingBarProgress, waitForContextInitialized);

            await loadingProgress;
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