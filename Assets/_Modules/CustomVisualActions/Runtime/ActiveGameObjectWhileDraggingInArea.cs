using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;
using VisualActions.Areas;


public class ActiveGameObjectWhileDraggingInArea : MonoDraggableExtension
{
    [SerializeField] private GameObject targetObj;
    [SerializeField] private BaseArea area;
    [SerializeField] private bool active;

    public override void StartDrag()
    {
    }

    public override void Drag()
    {
        if (this.area == null || !this.area.gameObject.activeInHierarchy)
        {
            return;
        }

        if (this.area.ContainsWorldSpace(BaseDraggable.Position))
        {
            this.targetObj.SetActive(this.active);
        }
        else
        {
            this.targetObj.SetActive(!this.active);
        }
    }

    public override void EndDrag()
    {
        this.targetObj.SetActive(!this.active);
    }
}