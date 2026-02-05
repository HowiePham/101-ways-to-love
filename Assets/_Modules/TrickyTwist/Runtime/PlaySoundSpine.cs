using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using Mimi.Prototypes;
using Mimi.ServiceLocators;
using Mimi.Services.ScriptableObject.Audio;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace VisualFlow
{
    public class PlaySoundSpine : VisualAction
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private BaseAudioServiceSO audioPlayer;
        private ServiceLocator ServiceLocator => ServiceLocator.Global;

        public SkeletonAnimation SkeletonAnimation
        {
            get => this.skeletonAnimation;
            set => this.skeletonAnimation = value;
        }

        [SpineEvent(dataField: "skeletonAnimation", fallbackToTextField: true)]
        public string eventName;

        [SerializeField, ValueDropdown("GetSoundGroups")]
        private string nameMusic;

        public string NameMusic
        {
            get => this.nameMusic;
            set => this.nameMusic = value;
        }

        public BaseAudioServiceSO AudioPlayer
        {
            get => this.audioPlayer;
            set => this.audioPlayer = value;
        }

        [SerializeField] private int track;
        [SerializeField] private bool noLoop;
        Spine.EventData eventData;
        [Space] public bool logDebugMessage = false;

        private int count;

        private bool outPlaysound = false;

        // Start is called before the first frame update
        private void StateOnEvent(TrackEntry trackentry, Spine.Event e)
        {
            this.eventData = skeletonAnimation.Skeleton.Data.FindEvent(eventName);
            this.count = 0;
            if (this.logDebugMessage) Debug.Log("Event fired! " + e.Data.Name);
            bool eventMatch = string.Equals(e.Data.Name, this.eventName, System.StringComparison.Ordinal); // Testing recommendation: String compare.
            if (eventMatch)
            {
                this.count++;
                if (this.noLoop && !this.outPlaysound)
                {
                    ServiceLocator.Get<IAudioService>().PlaySound(this.nameMusic);
                    // this.AudioPlayer.PlaySound(this.nameMusic);
                    this.outPlaysound = true;
                }

                if (!this.outPlaysound)
                {
                    ServiceLocator.Get<IAudioService>().PlaySound(this.nameMusic);
                    // this.AudioPlayer.PlaySound(this.nameMusic);
                }
            }
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.skeletonAnimation.state.Event += StateOnEvent;
            await UniTask.CompletedTask;
        }

        private void OnDisable()
        {
            // ServiceLocator.Get<IAudioService>().PlaySound(this.nameMusic, 0f);
            this.AudioPlayer.StopSound(this.nameMusic);
        }
        
#if UNITY_EDITOR
        private static IEnumerable<string> GetSoundGroups()
        {
            return MasterAudio.SafeInstance.GroupNames;
        }
#endif
    }
}