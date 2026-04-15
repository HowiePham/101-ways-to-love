using System;
using Cysharp.Threading.Tasks;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Data;
using UnityEngine;

public class ReturnStartPosition : MonoDraggableExtension
{
    [SerializeField] private Vector3Field offsetField;
    [SerializeField] private float delaySeconds;
    private bool isInitialized = false;
    private Vector3 startPosition;

    public override void Init(BaseDraggable draggable)
    {
        base.Init(draggable);

        this.startPosition = draggable.transform.position;

        if (this.offsetField == null)
        {
            draggable.TryGetComponent<Vector3Field>(out this.offsetField);
            if (this.offsetField == null)
            {
                this.offsetField = this.gameObject.AddComponent<Vector3Field>();
            }
        }

        this.isInitialized = true;
    }

    public override void StartDrag()
    {
    }

    public override void Drag()
    {
    }

    public override void EndDrag()
    {
        ReturnPos(this.BaseDraggable);
    }

    private void OnDisable()
    {
        if (!this.isInitialized)
        {
            return;
        }

        this.BaseDraggable.transform.position = this.startPosition;
    }

    async UniTask ReturnPos(BaseDraggable draggable)
    {
        await UniTask.Delay(Mathf.RoundToInt(this.delaySeconds * 1000));
        draggable.SetPosition(this.startPosition - this.offsetField.GetValue());
    }
}