using Cysharp.Threading.Tasks;
using Mimi.Audio;
using Mimi.VisualActions.Audio;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameScenes
{
    [RequireComponent(typeof(Timeline))]
    public class LevelPlayer : MonoBehaviour
    {
        [SerializeField] private Timeline timeline;
        [SerializeField] private PlayAudio[] levelGeneralAudio;

        public PlayAudio[] LevelGeneralAudio => this.levelGeneralAudio;

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