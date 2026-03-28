using DG.Tweening;
using Mimi.Prototypes.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterCellView : MonoBehaviour
{
    [SerializeField] protected Button selectBtn;
    [SerializeField] protected TMP_Text chapterOrderText;
    [SerializeField] protected Image lockIcon;
    [SerializeField] protected Image chapterIcon;
    [SerializeField] protected GameObject cell;
    [SerializeField] protected GameObject progressPanel;
    [SerializeField] protected GameObject highlightObject;
    [SerializeField] protected GameObject darkIconPanel;
    [SerializeField] protected Vector3 lockCellScale;

    [Header("Progress Bar")]
    [SerializeField] protected Image progressBarFill;
    [SerializeField] protected TMP_Text progressText;

    protected int chapterNumber;
    protected string iconAddress;
    protected CellStatus cellStatus;
    private Tweener pulseTween;

    protected void OnEnable()
    {
        this.selectBtn.onClick.AddListener(SelectChapter);
    }

    protected void OnDisable()
    {
        this.selectBtn.onClick.RemoveListener(SelectChapter);
        StopPulse();
    }

    public void SetData(int chapterNumber, string iconAddress, CellStatus cellStatus, float progress = 0f)
    {
        this.chapterNumber = chapterNumber;
        this.iconAddress = iconAddress;
        SetState(cellStatus);
        UpdateProgressBar(cellStatus, progress);
    }

    protected virtual void SetState(CellStatus cellStatus)
    {
        this.cellStatus = cellStatus;
        StopPulse();

        switch (cellStatus)
        {
            case CellStatus.Lock:
                this.cell.SetActive(true);
                this.chapterIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                SetButtonInteractable(false);
                SetActiveLockIcon(true);
                this.highlightObject.SetActive(false);
                this.cell.transform.localScale = this.lockCellScale;
                break;
            case CellStatus.Playing:
                this.chapterIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                this.cell.SetActive(true);
                SetButtonInteractable(true);
                SetActiveLockIcon(false);
                this.highlightObject.SetActive(true);
                this.cell.transform.localScale = Vector3.one;
                PlayPulse();
                break;
            case CellStatus.Complete:
                this.chapterIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                this.cell.SetActive(true);
                SetButtonInteractable(true);
                SetActiveLockIcon(false);
                this.highlightObject.SetActive(false);
                this.cell.transform.localScale = this.lockCellScale;
                break;
            case CellStatus.PlainCell:
                this.cell.SetActive(false);
                break;
        }
    }

    private void PlayPulse()
    {
        this.pulseTween = this.cell.transform.DOScale(1.08f, 0.6f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopPulse()
    {
        this.pulseTween?.Kill();
        this.pulseTween = null;
    }

    private void UpdateProgressBar(CellStatus status, float progress)
    {
        bool showProgress = status == CellStatus.Playing || status == CellStatus.Complete;

        if (this.progressBarFill != null)
        {
            this.progressBarFill.gameObject.SetActive(showProgress);
            this.progressBarFill.fillAmount = progress;
        }

        if (this.progressText != null)
        {
            this.progressText.gameObject.SetActive(showProgress);
            int percentage = Mathf.RoundToInt(progress * 100f);
            this.progressText.SetText(percentage + "%");
        }
    }

    public void SetChapterOrderText(int order, string chapterName)
    {
        this.chapterOrderText.SetText("Chapter " + order + ": " + chapterName);
    }

    protected virtual void SelectChapter()
    {
        Messenger.Broadcast(EventKey.SelectChapter, this);
    }

    protected void SetActiveLockIcon(bool active)
    {
        var lockIconGameObject = this.lockIcon.gameObject;
        lockIconGameObject.SetActive(active);
        this.darkIconPanel.SetActive(active);
        this.progressPanel.SetActive(!active);
    }

    protected void SetButtonInteractable(bool interactable)
    {
        this.selectBtn.interactable = interactable;
    }

    public int GetChapterOrder()
    {
        return this.chapterNumber;
    }

    public CellStatus GetChapterStatus()
    {
        return this.cellStatus;
    }
}
