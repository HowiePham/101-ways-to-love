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
    protected CellStatus cellStatus;

    protected void OnEnable()
    {
        this.selectBtn.onClick.AddListener(SelectChapter);
    }

    protected void OnDisable()
    {
        this.selectBtn.onClick.RemoveListener(SelectChapter);
    }

    public void SetData(int chapterNumber, CellStatus cellStatus, float progress = 0f)
    {
        this.chapterNumber = chapterNumber;
        SetState(cellStatus);
        UpdateProgressBar(cellStatus, progress);
    }

    protected virtual void SetState(CellStatus cellStatus)
    {
        this.cellStatus = cellStatus;

        switch (cellStatus)
        {
            case CellStatus.Lock:
                this.cell.SetActive(true);
                // this.levelIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                SetButtonInteractable(false);
                SetActiveLockIcon(true);
                this.highlightObject.SetActive(false);
                this.cell.transform.localScale = this.lockCellScale;
                break;
            case CellStatus.Playing:
                // this.levelIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                this.cell.SetActive(true);
                SetButtonInteractable(true);
                SetActiveLockIcon(false);
                this.highlightObject.SetActive(true);
                this.cell.transform.localScale = Vector3.one;
                break;
            case CellStatus.Complete:
                // this.levelIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
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

    public void SetChapterOrderText(int order)
    {
        this.chapterOrderText.SetText("Chapter " + order);
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
