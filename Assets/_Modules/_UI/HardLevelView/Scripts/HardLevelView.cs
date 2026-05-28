using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MEC;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HardLevelView : BaseView
{
    [SerializeField] private Transform smallTitle;
    [SerializeField] private Vector3 smallTitleStartPosition;

    [Title("Center group")] [SerializeField]
    private CanvasGroup centerIconGroup;

    [SerializeField] private Image background;

    [Title("Clock")] [SerializeField] private CanvasGroup clockGroup;
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private Transform clockGroupMessageTarget;

    [Title("Message Group")] [SerializeField]
    private CanvasGroup messageGroup;

    [SerializeField] private CanvasGroup darkBg;
    [SerializeField] private Image messageBoard;
    [SerializeField] private Transform deathGod;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button playButton;
    [SerializeField] private string messageString;

    [Title("Timeout")] [SerializeField] private GameObject timeoutGroup;
    [SerializeField] private GameObject timeoutPanel;
    [SerializeField] private Vector3 timeoutGroupStartLocalPosition;
    [SerializeField] private Button getMoreTimeButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Image timeoutWarning;
    [SerializeField] private TMP_Text additionalTimeText;

    [Title("SFX")] [SerializeField, SoundKey]
    private string hardLevelAudioKey;

    private Tween clockPulseTween;
    private Tween getMoreTimePulseTween;
    private Vector3 clockGroupOriginPosition;

    public event Action OnClickPlay;
    public event Action OnClickGetMoreTime;
    public event Action OnClickReplay;
    public event Action OnClickHome;

    private const string HardLevelShowFirstTimeDataKey = "hard_level_show_first_time";

    public override void Initialize()
    {
        base.Initialize();
        this.playButton.onClick.AddListener(() =>
        {
            StopClockPulse();
            this.clockGroup.transform.DOLocalMove(this.clockGroupOriginPosition, 0.4f).SetEase(Ease.OutQuint);
            OnClickPlay?.Invoke();
        });
        this.getMoreTimeButton.onClick.AddListener(() =>
        {
            StopGetMoreTimeButtonPulse();
            OnClickGetMoreTime?.Invoke();
        });
        this.replayButton.onClick.AddListener(() => OnClickReplay?.Invoke());
        this.homeButton.onClick.AddListener(() => OnClickHome?.Invoke());
    }

    public override void Show()
    {
        StopAllCoroutines();
        base.Show();
        StartCoroutine(ShowIntro());
    }

    public override void Hide()
    {
        StopAllCoroutines();
        DOTween.Kill(this.centerIconGroup);
        DOTween.Kill(this.background);
        // DOTween.Kill(this.warningFx.transform);
        DOTween.Kill(this.smallTitle);
        DOTween.Kill(this.clockGroup.transform);
        this.clockPulseTween?.Kill();
        this.clockPulseTween = null;
        this.getMoreTimePulseTween?.Kill();
        this.getMoreTimePulseTween = null;
        DOTween.Kill(this.messageGroup);
        DOTween.Kill(this.darkBg);
        DOTween.Kill(this.deathGod);
        DOTween.Kill(this.messageBoard);
        this.centerIconGroup.blocksRaycasts = false;
        base.Hide();
    }

    private IEnumerator ShowIntro()
    {
        if (!string.IsNullOrEmpty(this.hardLevelAudioKey))
        {
            ServiceLocator.Global.Get<IAudioService>().PlaySound(this.hardLevelAudioKey);
        }

        SetVisibilityWarningFX(false);
        Messenger.Broadcast(EventKey.PauseLevel, true);
        this.clockGroup.transform.localScale = Vector3.zero;
        this.playButton.transform.localScale = Vector3.zero;
        var smallTitleTargetPos = this.smallTitle.localPosition;
        this.smallTitle.localPosition = smallTitleStartPosition;
        ShowLogoHardLevel();
        yield return new WaitForSeconds(2.8f);
        ShowSmallTitle(smallTitleTargetPos);
        HideCenterIconGroup();
        ShowClockGroup();
        yield return new WaitForSeconds(0.6f);

        bool firstTimeShowingHardLevel = PlayerPrefs.GetInt(HardLevelShowFirstTimeDataKey) == 0;
        if (firstTimeShowingHardLevel)
        {
            PlayerPrefs.SetInt(HardLevelShowFirstTimeDataKey, 1);
            this.clockGroupOriginPosition = this.clockGroup.transform.localPosition;
            ShowMessageGroup();
        }
        else
        {
            OnClickPlay?.Invoke();
        }
    }

    private void ShowClockGroup()
    {
        DOTween.Sequence().Append(this.clockGroup.transform.DOScale(1, 0.6f).SetEase(Ease.OutBack));
    }

    private void StartClockPulse()
    {
        this.clockPulseTween = this.clockGroup.transform
            .DOScale(1.1f, 0.6f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopClockPulse()
    {
        this.clockPulseTween?.Kill();
        this.clockPulseTween = null;
        this.clockGroup.transform.localScale = Vector3.one;
    }

    private void StartGetMoreTimeButtonPulse()
    {
        this.getMoreTimePulseTween = this.getMoreTimeButton.transform
            .DOScale(1.08f, 0.7f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopGetMoreTimeButtonPulse()
    {
        this.getMoreTimePulseTween?.Kill();
        this.getMoreTimePulseTween = null;
        this.getMoreTimeButton.transform.localScale = Vector3.one;
    }

    private async UniTask ShowMessageGroup()
    {
        this.messageBoard.color = new Color(1, 1, 1, 0);
        SetMessageActive(true);
        this.messageText.SetText(string.Empty);
        this.messageGroup.alpha = 0;
        this.darkBg.alpha = 0;
        this.deathGod.localScale = Vector3.zero;
        DOTween.Sequence().Append(this.messageGroup.DOFade(1, 0.1f).SetEase(Ease.Linear));
        await DOTween.Sequence().Append(this.darkBg.DOFade(1, 0.1f).SetEase(Ease.Linear)).AsyncWaitForCompletion();
        await this.clockGroup.transform.DOLocalMove(this.clockGroupMessageTarget.localPosition, 0.4f).SetEase(Ease.OutQuint).AsyncWaitForCompletion();
        StartClockPulse();
        await DOTween.Sequence().Append(this.deathGod.DOScale(1, 0.3f).SetEase(Ease.OutBack)).AsyncWaitForCompletion();
        await DOTween.Sequence().Append(this.messageBoard.DOFade(1, 0.2f).SetEase(Ease.InQuart)).AsyncWaitForCompletion();
        await TextAppearEffect(this.messageString);
    }

    private async UniTask HideCenterIconGroup()
    {
        await DOTween.Sequence().Append(this.centerIconGroup.DOFade(0, 0.2f).SetEase(Ease.Linear)).AsyncWaitForCompletion();
        this.centerIconGroup.blocksRaycasts = false;
    }

    private void ShowSmallTitle(Vector3 smallTitleTargetPos)
    {
        DOTween.Sequence().Append(this.smallTitle.DOLocalMove(smallTitleTargetPos, 1).SetEase(Ease.OutQuint));
    }

    private void ShowLogoHardLevel()
    {
        this.centerIconGroup.blocksRaycasts = true;
        this.centerIconGroup.alpha = 1;
        var bgColor = this.background.color;
        var startBgAlpha = bgColor.a;
        bgColor.a = 0;
        this.background.color = bgColor;


        Sequence sequence = DOTween.Sequence();
        sequence.Append(this.background.DOFade(startBgAlpha, 1).SetEase(Ease.Linear));
    }

    private async UniTask TextAppearEffect(string text)
    {
        var characters = text.ToCharArray();
        var stringBuilder = new StringBuilder();
        foreach (var character in characters)
        {
            stringBuilder.Append(character);
            this.messageText.SetText(stringBuilder);
            await UniTask.WaitForSeconds(0.01f);
        }

        await UniTask.WaitForSeconds(0.2f);
        this.playButton.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack);
    }

    public void SetClockText(string text)
    {
        this.clockText.SetText(text);
    }

    public void SetPlayButtonActive(bool active)
    {
        this.playButton.gameObject.SetActive(active);
    }

    public void SetMessageActive(bool active)
    {
        this.messageGroup.gameObject.SetActive(active);
        this.darkBg.gameObject.SetActive(active);
    }

    public IEnumerator<float> TimeoutAppearFromTopEffect()
    {
        SetTimeOutGroupActive(true);
        this.getMoreTimeButton.transform.localScale = Vector3.zero;
        this.replayButton.transform.localScale = Vector3.zero;
        this.homeButton.transform.localScale = Vector3.zero;
        var originPosition = this.timeoutPanel.transform.localPosition;
        this.timeoutPanel.transform.localPosition = this.timeoutGroupStartLocalPosition;
        this.timeoutPanel.transform.DOLocalMove(originPosition, 0.4f).SetEase(Ease.OutBack);
        yield return Timing.WaitForSeconds(0.4f);
        this.getMoreTimeButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        yield return Timing.WaitForSeconds(0.1f);
        this.replayButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        yield return Timing.WaitForSeconds(0.1f);
        this.homeButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        yield return Timing.WaitForSeconds(0.2f);
        StartGetMoreTimeButtonPulse();
    }

    public void SetTimeOutGroupActive(bool active)
    {
        this.timeoutGroup.SetActive(active);
    }

    public IEnumerator<float> TimeoutWarningFX()
    {
        SetVisibilityWarningFX(true);
        DOTween.Sequence().Append(this.timeoutWarning.DOFade(1, 0.5f));
        DOTween.Sequence().Append(this.clockGroup.transform.DOScale(1.2f, 0.5f));
        yield return Timing.WaitForSeconds(0.5f);
        DOTween.Sequence().Append(this.timeoutWarning.DOFade(0, 0.5f));
        DOTween.Sequence().Append(this.clockGroup.transform.DOScale(1, 0.5f));
        yield return Timing.WaitForSeconds(0.5f);
    }

    public void SetVisibilityWarningFX(bool active)
    {
        this.timeoutWarning.gameObject.SetActive(active);
    }

    public void SetAdditionalTimeText(int time)
    {
        // this.additionalTimeText.SetText($"+{time}s");
    }
}