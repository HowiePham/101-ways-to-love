using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Games;
using MEC;
using Mimi;
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

    [SerializeField] private Image centerIcon;
    [SerializeField] private TMP_Text centerTitle;
    [SerializeField] private Transform lightingFx;
    [SerializeField] private Image background;

    [Title("Clock")] [SerializeField] private CanvasGroup clockGroup;
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private Image clockwise;

    [Title("Message Group")] [SerializeField]
    private CanvasGroup messageGroup;

    [SerializeField] private Image messageBoard;
    [SerializeField] private Transform deathGod;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button playButton;

    [Title("Timeout")] [SerializeField] private GameObject timeoutGroup;
    [SerializeField] private GameObject timeoutPanel;
    [SerializeField] private Vector3 timeoutGroupStartLocalPosition;
    [SerializeField] private Button getMoreTimeButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private Image timeoutWarning;
    [SerializeField] private TMP_Text additionalTimeText;

    public event Action OnClickPlay;
    public event Action OnClickGetMoreTime;
    public event Action OnClickReplay;

    public override void Initialize()
    {
        base.Initialize();
        this.playButton.onClick.AddListener(() => OnClickPlay?.Invoke());
        this.getMoreTimeButton.onClick.AddListener(() => OnClickGetMoreTime?.Invoke());
        this.replayButton.onClick.AddListener(() => OnClickReplay?.Invoke());
    }

    public override void Show()
    {
        base.Show();
        StartCoroutine(ShowIntro());
    }

    private IEnumerator ShowIntro()
    {
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

        // if (GameData.IsFirstTimeShowHardLevel)
        // {
        //     GameData.IsFirstTimeShowHardLevel = false;
        //     StartCoroutine(ShowMessageGroup());
        //     StartCoroutine(TextAppearEffect(message));
        // }
        // else
        // {
        OnClickPlay?.Invoke();
        // }
    }

    private void ShowClockGroup()
    {
        DOTween.Sequence().Append(this.clockGroup.transform.DOScale(1, 0.6f).SetEase(Ease.OutBack));
    }

    private IEnumerator ShowMessageGroup()
    {
        this.messageBoard.color = new Color(1, 1, 1, 0);
        this.SetMessageActive(true);
        this.messageText.SetText(string.Empty);
        this.messageGroup.alpha = 0;
        this.deathGod.localScale = Vector3.zero;
        DOTween.Sequence().Append(this.messageGroup.DOFade(1, 0.2f).SetEase(Ease.Linear));
        yield return new WaitForSeconds(0.2f);
        DOTween.Sequence().Append(this.deathGod.DOScale(1, 0.6f).SetEase(Ease.OutBack));
        yield return new WaitForSeconds(0.6f);
        DOTween.Sequence().Append(this.messageBoard.DOFade(1, 0.2f).SetEase(Ease.InQuart));
        yield return new WaitForSeconds(0.2f);
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
        this.centerIcon.transform.localScale = Vector3.zero;
        this.centerTitle.transform.localScale = Vector3.zero;
        this.lightingFx.transform.localScale = Vector3.zero;
        var bgColor = this.background.color;
        var startBgAlpha = bgColor.a;
        bgColor.a = 0;
        this.background.color = bgColor;
        DOTween.Sequence().Append(this.background.DOFade(startBgAlpha, 1).SetEase(Ease.Linear));
        DOTween.Sequence().Append(DOTween.To(value =>
        {
            var scale = new Vector3(value, value, value);
            this.centerIcon.transform.localScale = scale;
            this.centerTitle.transform.localScale = scale;
            this.lightingFx.transform.localScale = scale;
        }, 0, 0.8f, 2).SetEase(Ease.OutBack));
    }

    private IEnumerator TextAppearEffect(string text)
    {
        var characters = text.ToCharArray();
        var stringBuilder = new StringBuilder();
        foreach (var character in characters)
        {
            stringBuilder.Append(character);
            this.messageText.SetText(stringBuilder);
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.2f);
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
    }

    public void SetClockwise(float percent)
    {
        this.clockwise.fillAmount = percent;
    }

    public IEnumerator<float> TimeoutAppearFromTopEffect()
    {
        SetTimeOutGroupActive(true);
        this.getMoreTimeButton.transform.localScale = Vector3.zero;
        this.replayButton.transform.localScale = Vector3.zero;
        var originPosition = this.timeoutPanel.transform.localPosition;
        this.timeoutPanel.transform.localPosition = this.timeoutGroupStartLocalPosition;
        this.timeoutPanel.transform.DOLocalMove(originPosition, 0.4f).SetEase(Ease.OutBack);
        yield return Timing.WaitForSeconds(0.4f);
        this.getMoreTimeButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        yield return Timing.WaitForSeconds(0.1f);
        this.replayButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
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
        this.additionalTimeText.SetText($"+{time}s");
    }
}