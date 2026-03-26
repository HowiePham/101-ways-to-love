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
    [SerializeField] protected Vector3 lockCellScale;

    protected int levelOrder;
    protected string iconAddress;
    protected CellStatus cellStatus;

    protected static readonly Color32 lockColor = new Color32(125, 125, 125, 255);
    protected static readonly Color32 currentLevelColor = new Color32(170, 170, 170, 255);

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
                SetButtonInteractable(true);
                SetActiveLockIcon(true);
                // this.levelIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                this.levelIcon.color = lockColor;
                this.cell.transform.localScale = this.lockCellScale;
                break;
            case CellStatus.Playing:
                this.cell.SetActive(true);
                SetButtonInteractable(true);
                SetActiveLockIcon(false);
                // this.levelIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                this.levelIcon.color = Color.white;
                this.cell.transform.localScale = Vector3.one;
                break;
            case CellStatus.Complete:
                this.cell.SetActive(true);
                SetButtonInteractable(true);
                SetActiveLockIcon(false);
                // this.levelIcon.sprite = Resources.Load<Sprite>("Icons/" + this.iconAddress);
                this.levelIcon.color = Color.white;
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
        var progressPanelGameObject = progressPanel.gameObject;
        lockIconGameObject.SetActive(active);
        progressPanelGameObject.SetActive(!active);
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