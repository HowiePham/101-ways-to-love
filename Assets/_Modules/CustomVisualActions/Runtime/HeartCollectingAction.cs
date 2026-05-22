using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;

public class HeartCollectingAction : VisualAction
{
    [SerializeField] private Transform heartPool;
    [SerializeField] private Transform[] hearts;
    [SerializeField] private Transform angel;

    [FoldoutGroup("Fly")] [SerializeField] private float flyDuration = 0.45f;
    [FoldoutGroup("Fly")] [SerializeField] private float flyStagger = 0.08f;
    [FoldoutGroup("Fly")] [SerializeField] private Ease flyEase = Ease.InQuad;
    [FoldoutGroup("Fly")] [SerializeField] private float scaleDownDuration = 0.3f;

    protected override UniTask OnInitializing()
    {
        GatherHearts();

        return base.OnInitializing();
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (hearts == null || hearts.Length == 0)
            return;

        if (angel == null)
        {
            Debug.LogWarning("[HeartCollectingAction] Angel transform is not assigned.", this);
            return;
        }

        var flyTasks = new UniTask[hearts.Length];
        for (int i = 0; i < hearts.Length; i++)
            flyTasks[i] = FlyToAngel(hearts[i], i, cancellationToken);

        await UniTask.WhenAll(flyTasks);
    }

    private async UniTask FlyToAngel(Transform heart, int index, CancellationToken cancellationToken)
    {
        if (flyStagger > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(index * flyStagger), cancellationToken: cancellationToken);

        float scaleDownDelay = Mathf.Max(0f, flyDuration - scaleDownDuration);

        Sequence seq = DOTween.Sequence();
        seq.Join(heart.DOMove(angel.position, flyDuration).SetEase(flyEase));
        seq.Join(heart.DOScale(Vector3.zero, scaleDownDuration).SetEase(Ease.InBack).SetDelay(scaleDownDelay));

        await seq.AsyncWaitForCompletion();

        heart.gameObject.SetActive(false);
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