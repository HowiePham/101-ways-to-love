using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Interactions.Dragging;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mimi.VisualActions.Scanning
{
    public class VisualScan : VisualAction
    {
        [SerializeField] private BaseMonoScannerFactory scannerFactory;
        [SerializeField] private BaseDragHandler dragHandler;
        [SerializeField] private VisualCondition completeCondition;
        [SerializeField] private MonoDragExtension dragExtension;

        private DragInteraction dragInteraction;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            Assert.IsNotNull(this.scannerFactory, "Scanner Factory missing in game object " + gameObject.name);
            this.dragInteraction = new DragInteraction(this.dragHandler, this.completeCondition, this.dragExtension);
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            IScanner scanner = this.scannerFactory.CreateScanner();
            this.dragInteraction.Start();
            await UniTask.WaitUntil(() => this.dragInteraction.IsCompleted, PlayerLoopTiming.Update,
                cancellationToken);
            this.dragInteraction.Stop();
        }
    }
}