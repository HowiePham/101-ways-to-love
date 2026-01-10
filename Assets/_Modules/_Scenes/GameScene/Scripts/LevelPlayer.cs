using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameScenes
{
    [RequireComponent(typeof(Timeline))]
    public class LevelPlayer : MonoBehaviour
    {
        [SerializeField] private Timeline timeline;

        public async UniTask Play()
        {
            await this.timeline.Play();
        }

        [Button]
        private async void StartPlay()
        {
            await Play();
        }

        public void Cancel()
        {
            this.timeline.Cancel();
        }
    }
}