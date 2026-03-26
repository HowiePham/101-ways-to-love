using Mimi.Prototypes.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterCellView : MonoBehaviour
{
    [SerializeField] protected Button selectBtn;
    [SerializeField] protected TMP_Text chapterOrderText;
    [SerializeField] protected Image lockIcon;
    [SerializeField] protected GameObject cell;
    [SerializeField] protected GameObject progressPanel;
    [SerializeField] protected GameObject highlightObject;
    [SerializeField] protected GameObject darkIconPanel;
    [SerializeField] protected Vector3 lockCellScale;

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

    public void SetData(int chapterNumber, CellStatus cellStatus)
    {
        this.chapterNumber = chapterNumber;
        SetState(cellStatus);
    }

    protected virtual void SetState(CellStatus cellStatus)
    {
        this.cellStatus = cellStatus;

        switch (cellStatus)
        {
            case CellStatus.Lock:
                this.cell.SetActive(true);
                SetButtonInteractable(false);
                SetActiveLockIcon(true);
                this.highlightObject.SetActive(false);
                this.cell.transform.localScale = this.lockCellScale;
                break;
            case CellStatus.Playing:
                this.cell.SetActive(true);
                SetButtonInteractable(true);
                SetActiveLockIcon(false);
                this.highlightObject.SetActive(true);
                this.cell.transform.localScale = Vector3.one;
                break;
            case CellStatus.Complete:
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

    public void SetChapterOrderText(int order)
    {
        this.chapterOrderText.SetText(order.ToString());
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
