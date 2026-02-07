using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Common;
using Mimi.VisualActions;
using UnityEngine;

public class MoveGameObjectByTween : VisualAction
{
    [SerializeField] private Transform moveObject;
    [SerializeField] private Transform targetPos;
    [SerializeField] private float duration;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private bool vibrateOnMove;

    [SerializeField] private bool completeAfterMove = false;

    // [SerializeField, ValueDropdown("GetSoundGroups")]
    // private string moveSFX;

    public void SetMoveObject(Transform moveObject)
    {
        this.moveObject = moveObject;
    }

    public void SetDuration(float duration)
    {
        this.duration = duration;
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        var objLeanSelectable = GetLeanComponent(moveObject);

        var oldState = false;
        if (objLeanSelectable != null)
        {
            oldState = objLeanSelectable;
        }

        EnableObjectLeanSelectable(objLeanSelectable, false);

        if (vibrateOnMove)
        {
        }

        if (completeAfterMove)
        {
            await this.moveObject.DOMove(this.targetPos.position, this.duration).SetEase(this.ease).AsyncWaitForCompletion();
        }
        else
        {
            this.moveObject.DOMove(this.targetPos.position, this.duration).SetEase(this.ease);
        }

        EnableObjectLeanSelectable(objLeanSelectable, oldState);

        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        await UniTask.CompletedTask;
    }

    private void EnableObjectLeanSelectable(LeanSelectable objLeanSelectable, bool state)
    {
        if (objLeanSelectable == null) return;
        objLeanSelectable.enabled = state;
    }

    private LeanSelectable GetLeanComponent(Transform objTransform)
    {
        var leanSelectable = objTransform.GetComponent<LeanSelectable>();

        if (leanSelectable == null) leanSelectable = objTransform.GetComponentInChildren<LeanSelectable>();
        if (leanSelectable == null) leanSelectable = objTransform.GetComponentInParent<LeanSelectable>();

        return leanSelectable;
    }
}