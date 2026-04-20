using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualFlow;

public class TappingHint : BaseHint
{
    [SerializeField, Required] private BaseHintGraphic hintGraphic;

    protected override async UniTask OnInitializing()
    {
        await base.OnInitializing();
        EnableHint(false);
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        await base.OnExecuting(cancellationToken);
        EnableHint(true);

        try
        {
            await UniTask.WaitUntil(() =>
                Completed, PlayerLoopTiming.Update, cancellationToken);
        }
        catch (OperationCanceledException e)
        {
        }
        finally
        {
            StopListeningEvent();
            EnableHint(false);
            DOTween.Kill(this.gameObject);
        }
    }

    protected override void EnableHint(bool enable)
    {
        if (this.hintGraphic == null) return;
        this.hintGraphic.SetActive(enable);
        this.hintGraphic.enabled = enable;
    }
}