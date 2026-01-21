using System.Threading;
using Cysharp.Threading.Tasks;
using Spine.Unity;
using UnityEngine;
using VisualFlow;

public class ShowHintSpine : BaseHint
{
    [SerializeField] private GameObject hintObject;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField, SpineAnimation(dataField = "skeletonAnimation")]
    private new string animation;
    [SerializeField] private bool loop;
    [SerializeField] private int track;
    
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.hintObject.SetActive(true);
        this.skeletonAnimation.AnimationState.SetAnimation(this.track, this.animation, this.loop);
        await UniTask.WaitUntil(() => Completed, PlayerLoopTiming.Update,cancellationToken);
        this.hintObject.SetActive(false);
    }
}
