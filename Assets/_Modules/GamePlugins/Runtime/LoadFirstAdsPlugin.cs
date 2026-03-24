using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Ads.Adapters;
using Mimi.Events;
using Mimi.Games;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

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
        this.gameContext.Ads.Interstitial.OnLoadSucceeded += AdsLoadSucceeded;
        this.gameContext.Ads.Interstitial.OnLoadFailed += AdsLoadFailed;
        this.gameContext.Ads.RewardVideo.OnLoadSucceeded += AdsLoadSucceeded;
        this.gameContext.Ads.RewardVideo.OnLoadFailed += AdsLoadFailed;
    }

    private void AdsLoadFailed(AdError adError)
    {
        Debug.Log($"--- (ADS) Ads load FAILED: {adError.ErrorCode} --- {adError.Message}");
    }

    private void AdsLoadSucceeded()
    {
        Debug.Log($"--- (ADS) Ads load SUCCEEDED");
    }

    private async UniTask BootGameCompleted(BootGameCompleted bootGameCompleted,
        CancellationToken cancellationToken)
    {
        await UniTask.CompletedTask;
        Debug.Log($"--- (PLUGIN) Loading first ads inter...");
        this.gameContext.Ads.Interstitial.Load();
        await UniTask.Delay(3000, cancellationToken: cancellationToken);
        Debug.Log($"--- (PLUGIN) Loading first ads reward...");
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