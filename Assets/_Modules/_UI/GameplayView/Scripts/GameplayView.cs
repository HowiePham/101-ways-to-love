using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : BaseView
{
    [SerializeField] private GameObject progressObject;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Transform findTheCatNotification;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Vector3 notificationScale;
    [SerializeField] private float showingNotiDuration;

    public Action OnPauseClicked;

    public override void Initialize()
    {
        base.Initialize();
        this.pauseButton.onClick.AddListener(() => this.OnPauseClicked?.Invoke());
    }

    public void SetActiveProgress(bool value)
    {
        this.progressObject.SetActive(value);
    }

    [ContextMenu("Show Notification")]
    public async UniTask ShowFindTheCatNotification()
    {
        this.findTheCatNotification.gameObject.SetActive(true);
        await this.findTheCatNotification.DOScale(this.notificationScale, 0.3f).SetEase(Ease.OutCubic).AsyncWaitForCompletion();
        await UniTask.WaitForSeconds(this.showingNotiDuration);
        await this.findTheCatNotification.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutCubic).AsyncWaitForCompletion();
        this.findTheCatNotification.gameObject.SetActive(false);
    }

    public void SetTimerText(float timeLeft)
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);

        this.timerText.text = $"{minutes:D2}:{seconds:D2}";
    }

    public void SetProgressText(int currentProgress, int maxProgress)
    {
        this.progressText.text = currentProgress + "/" + maxProgress;
    }
}