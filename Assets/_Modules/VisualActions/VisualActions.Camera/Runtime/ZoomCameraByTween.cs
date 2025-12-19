using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Mimi.VisualActions.Camera.Runtime
{
    public class ZoomCameraByTween : VisualAction
    {
        [SerializeField] private UnityEngine.Camera camera;
        [SerializeField] private float targetSize;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Ease ease = Ease.Linear;
        
        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            if (camera == null)
            {
                camera = UnityEngine.Camera.main;
            }
        }
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            await camera.DOOrthoSize(targetSize, duration).SetEase(ease).AsyncWaitForCompletion().AsUniTask();
        }
    }
}