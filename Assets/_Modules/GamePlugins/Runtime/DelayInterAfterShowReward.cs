using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MEC;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using UnityEngine;

public class DelayInterAfterShowReward : IPlugin
{
    private GameContext gameContext;

    private CoroutineHandle cooldownCoroutine;
    private int levelWinCounter;

    public DelayInterAfterShowReward(GameContext gameContext)
    {
        this.gameContext = gameContext;
    }

    public async UniTask Install()
    {
        await UniTask.CompletedTask;
        this.gameContext.Ads.RewardVideo.OnRewarded += HandleOnVideoClosed;
        Messenger.AddListener(EventKey.LevelWin, LevelWindHandler);
    }

    private void LevelWindHandler()
    {
        this.levelWinCounter++;

        if (this.levelWinCounter >= 2)
        {
            this.gameContext.GameData.IsAdCoolDownCompletedAfterReward = true;
            this.levelWinCounter = 0;
        }
    }

    private void HandleOnVideoClosed(AdReward adReward)
    {
        this.levelWinCounter = 0;
        Timing.KillCoroutines(cooldownCoroutine);
        cooldownCoroutine = Timing.RunCoroutine(CooldownShowInter());
    }

    private IEnumerator<float> CooldownShowInter()
    {
        this.gameContext.GameData.IsAdCoolDownCompletedAfterReward = false;
        float cooldownDuration = this.gameContext.RemoteConfig.GetValue(ConfigKey.CooldownInterAfterShowReward).Float;
        Debug.Log($"--- (ADS) Start cooling down inter after rewarded ads in {cooldownDuration}s");
        yield return Timing.WaitForSeconds(cooldownDuration);
        this.gameContext.GameData.IsAdCoolDownCompletedAfterReward = true;
    }

    public async UniTask Uninstall()
    {
        await UniTask.CompletedTask;
        this.gameContext.Ads.RewardVideo.OnRewarded -= HandleOnVideoClosed;
        Messenger.RemoveListener(EventKey.LevelWin, LevelWindHandler);
        Timing.KillCoroutines(cooldownCoroutine);
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