using Mimi.Prototypes.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCellView : MonoBehaviour, ILevelCell
{
    [SerializeField] protected Button selectBtn;
    [SerializeField] protected TMP_Text levelOrderText;
    [SerializeField] protected Image levelIcon;
    [SerializeField] protected Image lockIcon;
    [SerializeField] protected GameObject cell;
    [SerializeField] protected GameObject progressPanel;
    [SerializeField] protected GameObject highlightObject;
    [SerializeField] protected GameObject darkIconPanel;
    [SerializeField] protected Vector3 lockCellScale;

    protected int levelOrder;
    protected string iconAddress;
    protected CellStatus cellStatus;

    protected void OnEnable()
    {
        this.selectBtn.onClick.AddListener(SelectLevel);
    }

    protected void OnDisable()
    {
        this.selectBtn.onClick.RemoveListener(SelectLevel);
    }

    public void SetData(int levelOrder, string iconAddress, CellStatus cellStatus)
    {
        this.levelOrder = levelOrder;
        this.iconAddress = iconAddress;
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
                // this.levelIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                this.cell.transform.localScale = this.lockCellScale;
                break;
            case CellStatus.Playing:
                this.cell.SetActive(true);
                SetButtonInteractable(true);
                SetActiveLockIcon(false);
                this.highlightObject.SetActive(true);
                // this.levelIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                this.cell.transform.localScale = Vector3.one;
                break;
            case CellStatus.Complete:
                this.cell.SetActive(true);
                SetButtonInteractable(true);
                SetActiveLockIcon(false);
                this.highlightObject.SetActive(false);
                // this.levelIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                this.cell.transform.localScale = this.lockCellScale;
                break;
            case CellStatus.PlainCell:
                this.cell.SetActive(false);
                break;
        }
    }

    public void SetLevelOrderText(int levelOrder)
    {
        this.levelOrderText.SetText((levelOrder + 1).ToString());
    }

    protected virtual void SelectLevel()
    {
        Messenger.Broadcast(EventKey.SelectLevel, this);
    }

    protected void SetActiveLockIcon(bool active)
    {
        var lockIconGameObject = lockIcon.gameObject;
        lockIconGameObject.SetActive(active);
        darkIconPanel.SetActive(active);
        progressPanel.SetActive(!active);
    }

    protected void SetButtonInteractable(bool interactable)
    {
        selectBtn.interactable = interactable;
    }

    public int GetLevelOrder()
    {
        return this.levelOrder;
    }

    public CellStatus GetLevelStatus()
    {
        return this.cellStatus;
    }
}