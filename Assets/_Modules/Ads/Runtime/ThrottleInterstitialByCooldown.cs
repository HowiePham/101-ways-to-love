using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Games;
using Mimi.Ads.Adapters;
using UnityEngine;


public class ThrottleInterstitialByCooldown : InterstitialDecorator
{
    private readonly GameData gameData;
    private readonly int cooldownMls;
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public ThrottleInterstitialByCooldown(IInterstitialAdapter adapter, float cooldownInSeconds, GameData gameData)
        : base(adapter)
    {
        this.gameData = gameData;
        this.cooldownMls = Mathf.CeilToInt(cooldownInSeconds * 1000f);
    }

    public override void Show(AdPlacement placement)
    {
        if (this.gameData.IsAdCoolDowning) return;
        base.Show(placement);
    }

    protected override void ClosedHandler(AdPlacement adPlacement)
    {
        base.ClosedHandler(adPlacement);
        StartCooldown();
        Debug.LogError("Start interstitial ad cooldown");
    }

    protected override void ShowSucceededHandler(AdPlacement placement)
    {
        base.ShowSucceededHandler(placement);
        this.gameData.IsAdCoolDowning = false;
        this.cancellationTokenSource?.Cancel();
    }

    private async UniTask StartCooldown()
    {
        this.gameData.IsAdCoolDowning = true;
        if (this.cancellationTokenSource != null)
        {
            this.cancellationTokenSource.Dispose();
        }

        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(this.cooldownMls, DelayType.UnscaledDeltaTime,
                cancellationToken: this.cancellationTokenSource.Token);
        }
        catch (OperationCanceledException e)
        {
            this.gameData.IsAdCoolDowning = false;
        }

        this.gameData.IsAdCoolDowning = false;
    }
}