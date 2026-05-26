using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;

public class HeartRisingUpAction : VisualAction
{
    [SerializeField] private Transform heartPool;
    [SerializeField] private Transform[] hearts;
    [SerializeField] private bool stopFloatingAfterComplete;

    [FoldoutGroup("Rise")] [SerializeField]
    private float floatUpOffset = 0.5f;

    [FoldoutGroup("Rise")] [SerializeField]
    private float floatUpDuration = 0.35f;

    [FoldoutGroup("Rise")] [SerializeField]
    private Ease floatUpEase = Ease.OutQuad;

    [FoldoutGroup("Rise")] [SerializeField]
    private float bouncePeakExtra = 0.2f;

    [FoldoutGroup("Rise")] [SerializeField]
    private float settleDownDuration = 0.2f;

    [FoldoutGroup("Rise")] [SerializeField]
    private float scaleOvershoot = 1.5f;

    [FoldoutGroup("Float")] [SerializeField]
    private float floatDuration = 1.5f;

    [FoldoutGroup("Float")] [SerializeField]
    private float floatAmplitude = 0.12f;

    [FoldoutGroup("Float")] [SerializeField]
    private float floatCycleDuration = 0.6f;

    private HeartState[] heartStates;

    protected override UniTask OnInitializing()
    {
        GatherHearts();

        if (hearts == null || hearts.Length == 0)
            return base.OnInitializing();

        heartStates = new HeartState[hearts.Length];
        for (int i = 0; i < hearts.Length; i++)
        {
            heartStates[i] = new HeartState
            {
                OriginalPos = hearts[i].position,
                OriginalScale = hearts[i].localScale
            };
            hearts[i].gameObject.SetActive(false);
        }

        return base.OnInitializing();
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (hearts == null || hearts.Length == 0)
            return;

        var riseAndFloatTasks = new UniTask[hearts.Length];
        for (int i = 0; i < hearts.Length; i++)
            riseAndFloatTasks[i] = RiseAndFloat(hearts[i], heartStates[i], cancellationToken);

        await UniTask.WhenAll(riseAndFloatTasks);
    }

    private async UniTask RiseAndFloat(Transform heart, HeartState state, CancellationToken cancellationToken)
    {
        heart.position = state.OriginalPos;
        heart.localScale = Vector3.zero;
        heart.gameObject.SetActive(true);

        Vector3 peakPos = state.OriginalPos + Vector3.up * (floatUpOffset + bouncePeakExtra);
        Vector3 finalPos = state.OriginalPos + Vector3.up * floatUpOffset;

        Sequence riseSeq = DOTween.Sequence();
        riseSeq.Append(heart.DOMove(peakPos, floatUpDuration).SetEase(floatUpEase));
        riseSeq.Append(heart.DOMove(finalPos, settleDownDuration).SetEase(Ease.InQuad));
        riseSeq.Insert(0, heart.DOScale(state.OriginalScale, floatUpDuration + settleDownDuration).SetEase(Ease.OutBack, scaleOvershoot));
        await riseSeq.AsyncWaitForCompletion();

        Tween bobTween = heart.DOLocalMoveY(heart.localPosition.y + floatAmplitude, floatCycleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(floatDuration), cancellationToken: cancellationToken);
        }
        finally
        {
            if (this.stopFloatingAfterComplete)
            {
                bobTween.Kill(complete: false);
            }
        }
    }

    private void OnDisable()
    {
        if (hearts == null) return;
        foreach (var heart in hearts)
        {
            if (heart != null) DOTween.Kill(heart);
        }
    }

    [Button("Gather Hearts")]
    private void GatherHearts()
    {
        var list = new List<Transform>();

        for (int i = 0; i < this.heartPool.childCount; i++)
        {
            list.Add(this.heartPool.GetChild(i));
        }

        this.hearts = list.ToArray();
    }
}