using System.Collections.Generic;
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

        [Header("Win Level Reference")] [SerializeField]
        private List<SkeletonAnimation> skeletonAnimations = new List<SkeletonAnimation>();

        [SerializeField] private GameObject levelUIRoot;

        [SerializeField, SpineAnimation(dataField = "skeletonAnimation")]
        protected List<string> winAnimation = new List<string>();

        [SerializeField] private bool loopWinAnim = true;
        [SerializeField] private Transform winCameraDestination;
        [SerializeField] private List<GameObject> disableObjectsWhenWin = new List<GameObject>();
        [SerializeField] private List<GameObject> activeObjectsWhenWin = new List<GameObject>();

        public PlayAudio[] LevelGeneralAudio => this.levelGeneralAudio;

        public async UniTask Play()
        {
            await this.timeline.Play();
        }

        public async UniTask EndLevel()
        {
            Cancel();

            if (this.disableObjectsWhenWin.Count > 0)
            {
                foreach (GameObject gameObject in this.disableObjectsWhenWin)
                {
                    gameObject.SetActive(false);
                }
            }
            if (this.activeObjectsWhenWin.Count > 0)
            {
                foreach (GameObject gameObject in this.activeObjectsWhenWin)
                {
                    gameObject.SetActive(true);
                }
            }

            this.levelUIRoot.SetActive(false);
            var levelEditor = GetComponent<LevelEditor>();

            for (var i = 0; i < this.skeletonAnimations.Count; i++)
            {
                SkeletonAnimation skeletonAnimation = this.skeletonAnimations[i];

                if (string.IsNullOrEmpty(this.winAnimation[i]) || i >= this.winAnimation.Count)
                {
                    continue;
                }

                skeletonAnimation.AnimationState.SetAnimation(0, this.winAnimation[i], this.loopWinAnim);
            }

            levelEditor.BoxInteractingObjectParent.gameObject.SetActive(false);
            levelEditor.InteractableObjectParent.gameObject.SetActive(false);
            levelEditor.DisableWhileRunningAnimation.gameObject.SetActive(false);
            levelEditor.HintParent.gameObject.SetActive(false);

            UpdateWinCameraPosition();
        }

        public void Cancel()
        {
            this.timeline.Cancel();
        }

        private void UpdateWinCameraPosition()
        {
            GameObject winCamera = GameObject.Find("WinCamera");
            if (winCamera == null || this.winCameraDestination == null)
            {
                return;
            }

            winCamera.transform.position = this.winCameraDestination.transform.position;
        }

        [Button]
        private async void StartPlay()
        {
            await Play();
        }

        [Button]
        private void GetWinAnimation()
        {
            this.winAnimation.Clear();
            var trueAnimLoops = GetComponentsInChildren<PlaySpineAnim>();

            foreach (PlaySpineAnim spineAnim in trueAnimLoops)
            {
                if (!spineAnim.gameObject.name.Contains("LoopAnim_True"))
                {
                    continue;
                }

                this.winAnimation.Add(spineAnim.Animation);
            }
        }

        [Button]
        private void GetWinCameraDestination()
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);

            foreach (Transform child in allChildren)
            {
                if (child.name == "WinCameraDestination")
                {
                    this.winCameraDestination = child;
                    break;
                }
            }
        }

        [Button]
        private void GetAllSpineAnim()
        {
            this.skeletonAnimations.Clear();
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);

            foreach (Transform child in allChildren)
            {
                if (!child.name.Contains("Anim"))
                {
                    continue;
                }

                var skeletonAnim = child.GetComponent<SkeletonAnimation>();
                if (skeletonAnim == null)
                {
                    continue;
                }

                this.skeletonAnimations.Add(skeletonAnim);
            }
        }
    }
}