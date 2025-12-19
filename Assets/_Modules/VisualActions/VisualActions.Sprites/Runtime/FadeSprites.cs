using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Sprites
{
    public class FadeSprites : VisualAction
    {
        [SerializeField, Required] private SpriteRenderer[] targets;
        [SerializeField, Range(0f, 1f)] private float startAlpha;
        [SerializeField, Range(0f, 1f)] private float endAlpha;
        [SerializeField] private Ease ease;
        [SerializeField, MinValue(0f)] private float duration = 1f;
        protected override UniTask OnExecuting(CancellationToken cancellationToken)
        {
            for (int i = 0; i < this.targets.Length; i++)
            {
                var target = this.targets[i];
                Color color = target.color;
                color.a = this.startAlpha;
                target.color = color;
                target.DOFade(this.endAlpha, this.duration).SetEase(this.ease);
            }
            
            return UniTask.CompletedTask;
        }
    }
}