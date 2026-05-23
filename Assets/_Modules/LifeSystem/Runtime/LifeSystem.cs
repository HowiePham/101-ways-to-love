using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Economy.Resources;
using IngameDebugConsole;
using MEC;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Configs;
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
    private readonly IConfigProvider remoteConfig;
    private readonly IResourceCollection playerResources;
    private readonly IAsyncPublisher publisher;
    private readonly IAsyncSubscriber subscriber;
    private readonly DialogManager dialogManager;
    private readonly IAdAdapter adAdapter;
    private readonly DisposableBag eventBag;
    private const string LifeDataKey = "LIFE";
    private const float FlushDebounceSeconds = 5f;
    private CoroutineHandle lifeTimerCoroutine;
    private CoroutineHandle flushCoroutine;
    private bool isLifeDataDirty;
    private YesNoDialog activeLifeDialog;
    private DialogId dialogId;
    private bool isInfiniteLife;

    public int CurrentLifeCount => (int)this.playerResources.GetAmount(LifeResourceId);

    public LifeSystem(int maxLifeCount, int timeToAddLifeInSeconds, IResourceCollection playerResources, IAsyncPublisher publisher, IAsyncSubscriber subscriber, DialogManager dialogManager,
        IAdAdapter adAdapter, IConfigProvider remoteConfig)
    {
        this.maxLifeCount = maxLifeCount;
        this.timeToAddLifeInSeconds = timeToAddLifeInSeconds;
        this.playerResources = playerResources;
        this.publisher = publisher;
        this.subscriber = subscriber;
        this.dialogManager = dialogManager;
        this.adAdapter = adAdapter;
        this.remoteConfig = remoteConfig;
        this.eventBag = new DisposableBag();
        this.subscriber.Subscribe<LifeUsing>(LifeUsingHandler).AddToBag(this.eventBag);
        this.adAdapter.RewardVideo.OnRewarded += OnLifeRewardCompleted;
        this.isInfiniteLife = false;

        Application.focusChanged += OnApplicationFocusChanged;
        Application.quitting += FlushLifeDataNow;

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
        DebugLogConsole.AddCommandInstance("infinite-life", "infinite life", "InfiniteLife", this);
    }

    private static bool TryParseDateTime(string str, out DateTime result)
    {
        if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            return true;
        return DateTime.TryParse(str, out result);
    }

    private async UniTask LifeUsingHandler(LifeUsing lifeUsing, CancellationToken token)
    {
        if (!AnyLifeLeft())
        {
            ShowGetMoreLifeDialog(DialogId.EndOfLifeDialog);
            return;
        }

        LooseLife(lifeUsing.Reason);
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

        int lifeAddAfterReward = this.remoteConfig.GetValue(ConfigKey.LifeAddAfterReward).Int;

        if (this.dialogManager.TryShowModalDialogOnce(dialogId, out this.activeLifeDialog))
        {
            Messenger.Broadcast(EventKey.PauseLevel, true);
            this.dialogId = dialogId;
            this.activeLifeDialog.SetContentText(lifeAddAfterReward.ToString());
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

        int lifeAddAfterReward = this.remoteConfig.GetValue(ConfigKey.LifeAddAfterReward).Int;
        string placement = this.dialogId == DialogId.EndOfLifeDialog ? "gameplay_view" : "select_level_view";
        AddLives(lifeAddAfterReward, placement, "get_more_life");
        ShowLifeChangedDialog($"+{lifeAddAfterReward}", true);

        if (this.lifeData.AddedNextTime.Count > 0)
        {
            this.lifeData.AddedNextTime.RemoveAt(this.lifeData.AddedNextTime.Count - 1);
            SaveLifeData();
        }

        RunTimer();

        Messenger.Broadcast(EventKey.PauseLevel, false);
        if (this.activeLifeDialog != null)
        {
            this.activeLifeDialog.Hide();
            this.activeLifeDialog = null;
        }
    }

    private void SaveLifeData()
    {
        this.lifeData.CurrentLifeCount = CurrentLifeCount;
        this.isLifeDataDirty = true;
        ScheduleFlush();
    }

    private void ScheduleFlush()
    {
        if (this.flushCoroutine != default) return;
        this.flushCoroutine = Timing.RunCoroutine(FlushAfterDelay());
    }

    private IEnumerator<float> FlushAfterDelay()
    {
        yield return Timing.WaitForSeconds(FlushDebounceSeconds);
        this.flushCoroutine = default;
        FlushLifeDataNow();
    }

    private void FlushLifeDataNow()
    {
        if (!this.isLifeDataDirty) return;
        PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(this.lifeData));
        PlayerPrefs.Save();
        this.isLifeDataDirty = false;
    }

    private void OnApplicationFocusChanged(bool hasFocus)
    {
        if (!hasFocus && this.isLifeDataDirty)
        {
            PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(this.lifeData));
            this.isLifeDataDirty = false;
        }
    }

    private void LooseLife(string reason)
    {
        if (this.isInfiniteLife)
        {
            return;
        }

        if (CurrentLifeCount > 0)
        {
            this.playerResources.Sink(LifeResourceId, 1, TransactionInfo.New(LifeResourceId, "gameplay_view", reason));
            if (CurrentLifeCount < this.maxLifeCount)
                SetTimeToAddNextLife();
            SaveLifeData();
            this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount));

            ShowLifeChangedDialog("-1", false);
        }
    }

    private void AddLife(string placement, string reason)
    {
        if (CurrentLifeCount < this.maxLifeCount)
        {
            this.playerResources.Source(LifeResourceId, 1, TransactionInfo.New(LifeResourceId, placement, reason));
            SaveLifeData();
            this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount));
        }
    }

    public void RefillLife()
    {
        this.playerResources.SetAmount(LifeResourceId, this.maxLifeCount, TransactionInfo.New(LifeResourceId, "LifeSystem", "refill"));
        this.lifeData.AddedNextTime = new List<string>();
        SaveLifeData();
    }

    public void AddLives(int count, string placement, string reason)
    {
        if (count <= 0) return;

        this.playerResources.Source(LifeResourceId, count,
            TransactionInfo.New(LifeResourceId, placement, reason));

        if (CurrentLifeCount >= this.maxLifeCount)
        {
            this.lifeData.AddedNextTime.Clear();
        }
        else
        {
            int timersToRemove = Mathf.Min(count, this.lifeData.AddedNextTime.Count);
            for (int i = 0; i < timersToRemove; i++)
                this.lifeData.AddedNextTime.RemoveAt(this.lifeData.AddedNextTime.Count - 1);
        }

        SaveLifeData();
        this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount));
        RunTimer();
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

        if (!TryParseDateTime(this.lifeData.AddedNextTime[0], out DateTime parsedTime))
            return "";
        TimeSpan span = parsedTime - DateTime.Now;
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
            if (!TryParseDateTime(times, out DateTime baseTime))
                baseTime = DateTime.Now;
            DateTime nextTime = baseTime.AddSeconds(seconds);
            this.lifeData.AddedNextTime.Add(nextTime.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            DateTime nextTime = DateTime.Now.AddSeconds(seconds);
            this.lifeData.AddedNextTime.Add(nextTime.ToString(CultureInfo.InvariantCulture));
        }

        SaveLifeData();
    }

    private void CheckLife()
    {
        for (var i = 0; i < this.lifeData.AddedNextTime.Count; i++)
        {
            string nextTime = this.lifeData.AddedNextTime[i];
            if (!TryParseDateTime(nextTime, out DateTime parsedTime))
            {
                this.lifeData.AddedNextTime.RemoveAt(i);
                i--;
                continue;
            }

            TimeSpan span = parsedTime - DateTime.Now;

            if (span.TotalSeconds < 0)
            {
                this.lifeData.AddedNextTime.RemoveAt(0);
                AddLife("life_system", "refill_life");
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
                if (TryParseDateTime(this.lifeData.AddedNextTime[0], out DateTime parsedTime))
                {
                    TimeSpan span = parsedTime - DateTime.Now;
                    this.publisher.PublishAsync(new RecoveryLifeTimerUpdated(GetRemainingTime(span)));
                    if (span.TotalSeconds < 0)
                    {
                        this.lifeData.AddedNextTime.RemoveAt(0);
                        AddLife("life_system", "refill_life");
                    }
                }
            }

            yield return Timing.WaitForOneFrame;
        }

        this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount));
        this.publisher.PublishAsync(new RecoveryLifeTimerUpdated(""));
    }

    private void InfiniteLife()
    {
        this.isInfiniteLife = true;
    }

    private void ShowLifeChangedDialog(string changedString, bool increased)
    {
        var spriteString = increased ? "<sprite index=0>" : "<sprite index=1>";
        if (this.dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide, out AutoHideNotificationDialog dialog))
        {
            dialog.SetText(changedString + " " + spriteString);
        }
    }
}