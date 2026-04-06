using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Economy.Resources;
using IngameDebugConsole;
using MEC;
using Mimi.Ads.Adapters;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.UI;
using UnityEngine;

public class LifeSystem
{
    private const string LifeResourceId = "Life";
    private readonly int maxLifeCount;
    private readonly int timeToAddLifeInSeconds;
    private readonly LifeData lifeData;
    private readonly IResourceCollection playerResources;
    private readonly IAsyncPublisher publisher;
    private readonly IAsyncSubscriber subscriber;
    private readonly DialogManager dialogManager;
    private readonly IAdAdapter adAdapter;
    private readonly DisposableBag eventBag;
    private const string LifeDataKey = "LIFE";
    private CoroutineHandle lifeTimerCoroutine;
    private YesNoDialog activeLifeDialog;
    private DialogId dialogId;

    public int CurrentLifeCount => (int)this.playerResources.GetAmount(LifeResourceId);

    public LifeSystem(int maxLifeCount, int timeToAddLifeInSeconds, IResourceCollection playerResources, IAsyncPublisher publisher, IAsyncSubscriber subscriber, DialogManager dialogManager,
        IAdAdapter adAdapter)
    {
        this.maxLifeCount = maxLifeCount;
        this.timeToAddLifeInSeconds = timeToAddLifeInSeconds;
        this.playerResources = playerResources;
        this.publisher = publisher;
        this.subscriber = subscriber;
        this.dialogManager = dialogManager;
        this.adAdapter = adAdapter;
        this.eventBag = new DisposableBag();
        this.subscriber.Subscribe<LifeUsing>(LifeUsingHandler).AddToBag(this.eventBag);
        this.adAdapter.RewardVideo.OnRewarded += OnLifeRewardCompleted;

        if (PlayerPrefs.HasKey(LifeDataKey))
        {
            this.lifeData = JsonUtility.FromJson<LifeData>(PlayerPrefs.GetString(LifeDataKey));
            this.playerResources.SetAmount(LifeResourceId, this.lifeData.CurrentLifeCount, TransactionInfo.New(LifeResourceId, "LifeSystem", "Restore"));
        }
        else
        {
            this.lifeData = new LifeData { CurrentLifeCount = maxLifeCount };
            this.playerResources.SetAmount(LifeResourceId, maxLifeCount, TransactionInfo.New(LifeResourceId, "LifeSystem", "Init"));
            SaveLifeData();
        }

        CheckLife();
        DebugLogConsole.AddCommandInstance("add-life", "Add 1 Life", "AddLife", this);
    }

    private async UniTask LifeUsingHandler(LifeUsing lifeUsing, CancellationToken token)
    {
        if (!AnyLifeLeft())
        {
            ShowGetMoreLifeDialog(DialogId.EndOfLifeDialog);
            return;
        }

        LooseLife();
        RunTimer();

        if (!AnyLifeLeft())
        {
            ShowGetMoreLifeDialog(DialogId.EndOfLifeDialog);
        }

        await UniTask.CompletedTask;
    }

    public void ShowGetMoreLifeDialog(DialogId dialogId)
    {
        if (IsLifeIsFull())
        {
            return;
        }

        if (this.dialogManager.TryShowModalDialogOnce(dialogId, out this.activeLifeDialog))
        {
            Messenger.Broadcast(EventKey.PauseLevel, true);
            this.dialogId = dialogId;
            this.activeLifeDialog.SetYesCallback(OnGetMoreLifeClicked);
            this.activeLifeDialog.SetNoCallback(CloseGetMoreLifeDialog);
        }
    }

    private void CloseGetMoreLifeDialog()
    {
        Messenger.Broadcast(EventKey.PauseLevel, false);
    }

    private void OnGetMoreLifeClicked()
    {
        if (this.adAdapter.RewardVideo.IsReady)
        {
            this.publisher.PublishAsync(new _Modules.GameEvent.Scripts.AdShowRequested());
            this.adAdapter.RewardVideo.Show(new AdReward("extra_life"), new AdPlacement("life_system"));
        }
        else
        {
            ShowAdFailedDialog();
        }
    }

    private void ShowAdFailedDialog()
    {
        if (this.dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide,
                out AutoHideNotificationDialog dialog))
        {
            dialog.SetText("Ads is not available");
        }
    }

    private void OnLifeRewardCompleted(AdReward reward)
    {
        if (reward.RewardId != "extra_life") return;

        AddLife();
        if (this.lifeData.AddedNextTime.Count > 0)
        {
            this.lifeData.AddedNextTime.RemoveAt(this.lifeData.AddedNextTime.Count - 1);
            SaveLifeData();
        }

        RunTimer();

        Messenger.Broadcast(EventKey.PauseLevel, false);
        this.activeLifeDialog.Hide();
        this.activeLifeDialog = null;
    }

    private void SaveLifeData()
    {
        this.lifeData.CurrentLifeCount = CurrentLifeCount;
        PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(this.lifeData));
    }

    private void LooseLife()
    {
        if (CurrentLifeCount > 0)
        {
            this.playerResources.Sink(LifeResourceId, 1, TransactionInfo.New(LifeResourceId, "gameplay_view", "lose_life"));
            SetTimeToAddNextLife();
            SaveLifeData();
            this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount));
        }
    }

    private void AddLife()
    {
        if (CurrentLifeCount < this.maxLifeCount)
        {
            var placement = this.dialogId == DialogId.EndOfLifeDialog ? "gameplay_view" : "select_level_view";
            this.playerResources.Source(LifeResourceId, 1, TransactionInfo.New(LifeResourceId, placement, "get_more_life"));
            SaveLifeData();
            this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount));
        }
    }

    public void RefillLife()
    {
        this.playerResources.SetAmount(LifeResourceId, this.maxLifeCount, TransactionInfo.New(LifeResourceId, "LifeSystem", "Refill"));
        this.lifeData.AddedNextTime = new List<string>();
        SaveLifeData();
    }

    public string GetRemainingTime(TimeSpan timeSpan)
    {
        string time = String.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);

        return time;
    }

    public string GetRemainingTime()
    {
        if (this.lifeData.AddedNextTime.Count <= 0)
        {
            return "";
        }

        TimeSpan span = DateTime.Parse(this.lifeData.AddedNextTime[0]) - DateTime.Now;
        return GetRemainingTime(span);
    }

    public bool IsLifeIsFull()
    {
        return CurrentLifeCount >= this.maxLifeCount;
    }

    public bool AnyLifeLeft()
    {
        return CurrentLifeCount > 0;
    }

    private void SetTimeToAddNextLife()
    {
        var seconds = this.timeToAddLifeInSeconds;
        if (this.lifeData.AddedNextTime.Count > 0)
        {
            string times = this.lifeData.AddedNextTime[lifeData.AddedNextTime.Count - 1];
            DateTime nextTime = DateTime.Parse(times).AddSeconds(seconds);
            this.lifeData.AddedNextTime.Add(nextTime.ToString());
        }
        else
        {
            DateTime nextTime = DateTime.Now.AddSeconds(seconds);
            this.lifeData.AddedNextTime.Add(nextTime.ToString());
        }

        SaveLifeData();
    }

    private void CheckLife()
    {
        for (var i = 0; i < this.lifeData.AddedNextTime.Count; i++)
        {
            string nextTime = this.lifeData.AddedNextTime[i];
            TimeSpan span = DateTime.Parse(nextTime) - DateTime.Now;

            if (span.TotalSeconds < 0)
            {
                this.lifeData.AddedNextTime.RemoveAt(0);
                AddLife();
                i--;
            }
            else
            {
                break;
            }
        }

        if (IsLifeIsFull() && this.lifeData.AddedNextTime.Count > 0)
        {
            this.lifeData.AddedNextTime.Clear();
        }

        int livesNeeded = this.maxLifeCount - CurrentLifeCount;
        int timersShortfall = livesNeeded - this.lifeData.AddedNextTime.Count;
        for (int i = 0; i < timersShortfall; i++)
        {
            SetTimeToAddNextLife();
        }

        SaveLifeData();
    }

    public void RunTimer()
    {
        StopTimer();
        this.lifeTimerCoroutine = Timing.RunCoroutine(LifeRecoveringTimer());
    }

    private void StopTimer()
    {
        if (this.lifeTimerCoroutine == default)
        {
            return;
        }

        Timing.KillCoroutines(this.lifeTimerCoroutine);
        this.lifeTimerCoroutine = default;
    }

    private IEnumerator<float> LifeRecoveringTimer()
    {
        while (CurrentLifeCount < this.maxLifeCount)
        {
            if (this.lifeData.AddedNextTime.Count > 0)
            {
                TimeSpan span = DateTime.Parse(this.lifeData.AddedNextTime[0]) - DateTime.Now;
                this.publisher.PublishAsync(new RecoveryLifeTimerUpdated(GetRemainingTime(span)));
                if (span.TotalSeconds < 0)
                {
                    this.lifeData.AddedNextTime.RemoveAt(0);
                    AddLife();
                }
            }

            yield return Timing.WaitForOneFrame;
        }

        this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount));
        this.publisher.PublishAsync(new RecoveryLifeTimerUpdated(""));
    }
}