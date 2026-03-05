using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Dragging
{
    public class WaitInsideArea2D : VisualAction
    {
        [SerializeField] protected Transform checkTransform;
        [SerializeField] protected BaseArea targetArea;
        [SerializeField] private float insideDuration;
        private float currentTime;
        private bool isComplete;
        public Transform CheckTransform => this.checkTransform;
        public BaseArea TargetArea => this.targetArea;

        private void Update()
        {
            if (!this.IsExecuting || this.isComplete)
            {
                return;
            }

            if (!this.TargetArea.Active || !this.checkTransform.gameObject.activeSelf) return;
            Vector3 checkPos = this.CheckTransform.position;

            if (this.TargetArea.ContainsWorldSpace(checkPos))
            {
                this.currentTime += Time.deltaTime;
                if (this.currentTime >= this.insideDuration)
                {
                    this.isComplete = true;
                }
            }
            else
            {
                this.currentTime = 0;
            }
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.currentTime = 0;
            this.isComplete = false;

            await UniTask.WaitUntil(() => this.isComplete);
        }
    }
}