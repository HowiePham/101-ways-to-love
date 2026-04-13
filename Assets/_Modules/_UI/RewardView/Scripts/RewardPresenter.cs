using Cysharp.Threading.Tasks;
using Mimi;
using Mimi.Configs;
using Mimi.Prototypes.UI;
using UnityEngine;

public class RewardPresenter : BaseViewPresenter
{
    private RewardView rewardView;
    private readonly IConfigProvider remoteConfig;

    public RewardPresenter(BaseScenePresenter scenePresenter, Transform transform,
        IConfigProvider remoteConfig) : base(scenePresenter, transform)
    {
        this.remoteConfig = remoteConfig;
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
