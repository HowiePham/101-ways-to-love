using _Modules.GameEvent.Scripts;
using Cysharp.Threading.Tasks;
using Mimi;
using Mimi.Configs;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes.UI;
using UnityEngine;

public class RewardPresenter : BaseViewPresenter
{
    private RewardView rewardView;
    private readonly IConfigProvider remoteConfig;
    private readonly IAsyncPublisher eventPublisher;

    public RewardPresenter(BaseScenePresenter scenePresenter, Transform transform,
        IConfigProvider remoteConfig, IAsyncPublisher eventPublisher) : base(scenePresenter, transform)
    {
        this.remoteConfig = remoteConfig;
        this.eventPublisher = eventPublisher;
    }

    protected override void AddViews()
    {
        this.rewardView = AddView<RewardView>();
    }

    protected override void AddChildren()
    {
    }

    public async UniTask ShowAndWait()
    {
        this.eventPublisher.PublishAsync(new DestroyLevelRequested());

        int rewardAmount = this.remoteConfig.GetValue(ConfigKey.LifeRecoverAfterChapter).Int;
        this.rewardView.SetData(rewardAmount);

        var tcs = new UniTaskCompletionSource();
        void OnComplete() => tcs.TrySetResult();
        this.rewardView.OnAnimationCompleted += OnComplete;
        Show();
        await tcs.Task;
        this.rewardView.OnAnimationCompleted -= OnComplete;
        Hide();
    }
}