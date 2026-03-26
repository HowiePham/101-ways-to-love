using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MEC;
using Mimi.Ads.Adapters;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.UI;
using Unity.VisualScripting;
using UnityEngine;

public class LifeSystem
{
    private readonly int maxLifeCount;
    private readonly int timeToAddLifeInSeconds;
    private readonly LifeData lifeData;
    private readonly IAsyncPublisher publisher;
    private readonly IAsyncSubscriber subscriber;
    private readonly DialogManager dialogManager;
    private readonly IAdAdapter adAdapter;
    private readonly DisposableBag eventBag;
    private const string LifeDataKey = "LIFE";
    private CoroutineHandle lifeTimerCoroutine;
    private YesNoDialog activeLifeDialog;

    public int CurrentLifeCount
    {
        get => this.lifeData.CurrentLifeCount;
        private set => this.lifeData.CurrentLifeCount = value;
    }

    public LifeSystem(int maxLifeCount, int timeToAddLifeInSeconds, IAsyncPublisher publisher, IAsyncSubscriber subscriber, DialogManager dialogManager, IAdAdapter adAdapter)
    {
        this.maxLifeCount = maxLifeCount;
        this.timeToAddLifeInSeconds = timeToAddLifeInSeconds;
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
        }
        else
        {
            this.lifeData = new LifeData
            {
                CurrentLifeCount = maxLifeCount
            };
            PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(this.lifeData));
        }

        CheckLife();
    }

    private async UniTask LifeUsingHandler(LifeUsing lifeUsing, CancellationToken token)
    {
        if (!AnyLifeLeft())
        {
            Debug.Log($"--- (LIFE) Do not have any Life left!");
            return;
        }

        LooseLife();
        RunTimer();

        if (!AnyLifeLeft())
        {
            ShowGetMoreLifeDialog();
        }

        await UniTask.CompletedTask;
    }

    private void ShowGetMoreLifeDialog()
    {
        if (this.dialogManager.TryShowModalDialogOnce<YesNoDialog>(DialogId.LifeDialog, out this.activeLifeDialog))
        {
            this.activeLifeDialog.SetContentText("Get more life");
            this.activeLifeDialog.SetYesText("+1 Life");
            this.activeLifeDialog.SetNoText("Close");
            this.activeLifeDialog.SetYesCallback(OnGetMoreLifeClicked);
        }
    }

    private void OnGetMoreLifeClicked()
    {
        if (this.adAdapter.RewardVideo.IsReady)
        {
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
        this.lifeData.AddedNextTime.RemoveAt(this.lifeData.AddedNextTime.Count - 1);
        RunTimer();
        
        this.activeLifeDialog.Hide();
        this.activeLifeDialog = null;
    }

    private void LooseLife()
    {
        if (CurrentLifeCount > 0)
        {
            CurrentLifeCount--;
            PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(this.lifeData));
            SetTimeToAddNextLife();
            this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount));
        }
    }

    private void AddLife()
    {
        if (CurrentLifeCount < this.maxLifeCount)
        {
            this.lifeData.CurrentLifeCount += 1;
            PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(this.lifeData));
            this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount));
        }
    }

    public void RefillLife()
    {
        CurrentLifeCount = this.maxLifeCount;
        this.lifeData.AddedNextTime = new List<string>();
        PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(this.lifeData));
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
            return "Full";
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

        PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(lifeData));
    }

    private void CheckLife()
    {
        if (this.lifeData.AddedNextTime.Count <= 0)
        {
            return;
        }

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
        this.publisher.PublishAsync(new RecoveryLifeTimerUpdated("Full"));
    }
}