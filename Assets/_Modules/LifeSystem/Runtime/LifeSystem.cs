using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MEC;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using UnityEngine;

public class LifeSystem
{
    private readonly int maxLifeCount;
    private readonly int timeToAddLifeInSeconds;
    private readonly LifeData lifeData;
    private readonly IAsyncPublisher publisher;
    private readonly IAsyncSubscriber subscriber;
    private readonly DisposableBag eventBag;
    private const string LifeDataKey = "LIFE";
    private CoroutineHandle lifeTimerCoroutine;

    public int CurrentLifeCount
    {
        get => this.lifeData.CurrentLifeCount;
        private set => this.lifeData.CurrentLifeCount = value;
    }

    public LifeSystem(int maxLifeCount, int timeToAddLifeInSeconds, IAsyncPublisher publisher, IAsyncSubscriber subscriber)
    {
        this.maxLifeCount = maxLifeCount;
        this.timeToAddLifeInSeconds = timeToAddLifeInSeconds;
        this.publisher = publisher;
        this.subscriber = subscriber;
        this.eventBag = new DisposableBag();
        this.subscriber.Subscribe<LifeUsing>(LifeUsingHandler).AddToBag(this.eventBag);

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
        LooseLife();
        RunTimer();
        await UniTask.CompletedTask;
    }

    private void LooseLife()
    {
        if (CurrentLifeCount > 0)
        {
            CurrentLifeCount--;
            PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(this.lifeData));
            SetTimeToAddNextLife();
        }
    }

    private void AddLife()
    {
        if (CurrentLifeCount < this.maxLifeCount)
        {
            this.lifeData.CurrentLifeCount += 1;
            PlayerPrefs.SetString(LifeDataKey, JsonUtility.ToJson(this.lifeData));
        }
    }

    public void RefillLife()
    {
        CurrentLifeCount = this.maxLifeCount;
        this.lifeData.AddedNextTime = new System.Collections.Generic.List<string>();
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

    public bool CanPlay()
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
        if (this.lifeData.AddedNextTime.Count > 0)
        {
            string times = this.lifeData.AddedNextTime[lifeData.AddedNextTime.Count - 1];
            TimeSpan span = DateTime.Parse(times) - DateTime.Now;
            this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount, GetRemainingTime(span)));
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
                this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount, GetRemainingTime(span)));
                if (span.TotalSeconds < 0)
                {
                    this.lifeData.AddedNextTime.RemoveAt(0);
                    AddLife();
                }
            }

            yield return Timing.WaitForSeconds(0.5f);
        }

        this.publisher.PublishAsync(new LifeUpdated(CurrentLifeCount, "Full"));
    }
}