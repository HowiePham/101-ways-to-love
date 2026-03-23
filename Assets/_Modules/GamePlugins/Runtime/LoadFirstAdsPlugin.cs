using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Events;
using Mimi.Games;
using Mimi.Games.Plugins;
using Mimi.Prototypes;

public class LoadFirstAdsPlugin : IPlugin
{
    private GameContext gameContext;

    private readonly DisposableBag disposeBag = new();

    public LoadFirstAdsPlugin(GameContext gameContext)
    {
        this.gameContext = gameContext;
    }

    public async UniTask Install()
    {
        await UniTask.CompletedTask;
        this.gameContext.EventSubscriber.Subscribe<BootGameCompleted>(BootGameCompleted).AddToBag(this.disposeBag);
    }

    private async UniTask BootGameCompleted(BootGameCompleted bootGameCompleted,
        CancellationToken cancellationToken)
    {
        await UniTask.CompletedTask;
        this.gameContext.Ads.Interstitial.Load();
        await UniTask.Delay(3000, cancellationToken: cancellationToken);
        this.gameContext.Ads.RewardVideo.Load();
    }

    public async UniTask Uninstall()
    {
        await UniTask.CompletedTask;
        this.disposeBag.Dispose();
    }

    public async UniTask Begin()
    {
        await UniTask.CompletedTask;
    }

    public async UniTask End()
    {
        await UniTask.CompletedTask;
    }
}