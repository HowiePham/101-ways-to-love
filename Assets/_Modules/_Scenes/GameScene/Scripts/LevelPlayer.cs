using Cysharp.Threading.Tasks;
using Mimi.Audio;
using Mimi.VisualActions.Audio;
using Mimi.VisualActions.Spines;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace GameScenes
{
    [RequireComponent(typeof(Timeline))]
    public class LevelPlayer : MonoBehaviour
    {
        [SerializeField] private Timeline timeline;
        [SerializeField] private PlayAudio[] levelGeneralAudio;
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField, SpineAnimation(dataField = "skeletonAnimation")]
        protected new string winAnimation;
        [SerializeField] private bool loopWinAnim = true;

        public PlayAudio[] LevelGeneralAudio => this.levelGeneralAudio;

        public async UniTask Play()
        {
            await this.timeline.Play();
        }

        public async UniTask EndLevel()
        {
            var levelEditor = GetComponent<LevelEditor>();
            this.skeletonAnimation.AnimationState.SetAnimation(0, this.winAnimation, this.loopWinAnim);

            levelEditor.BoxInteractingObjectParent.gameObject.SetActive(false);
            levelEditor.InteractableObjectParent.gameObject.SetActive(false);
            levelEditor.DisableWhileRunningAnimation.gameObject.SetActive(false);
            levelEditor.HintParent.gameObject.SetActive(false);
        }

        [Button]
        private async void StartPlay()
        {
            await Play();
        }

        [Button]
        private void GetWinAnimation()
        {
            var trueAnimLoops = GetComponentsInChildren<PlaySpineAnim>();

            foreach (PlaySpineAnim spineAnim in trueAnimLoops)
            {
                if (!spineAnim.gameObject.name.Contains("LoopAnim_True"))
                {
                    continue;
                }

                this.winAnimation = spineAnim.Animation;
                return;
            }
        }

        public void Cancel()
        {
            this.timeline.Cancel();
        }
    }
}