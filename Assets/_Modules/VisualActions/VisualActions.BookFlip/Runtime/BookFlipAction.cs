using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.BookFlip
{
    [TypeInfoBox("This action describes flipping a SpriteRenderer in one of four specific directions: left -> right, right -> left, top -> bottom, or bottom -> top.")]
    public class BookFlipAction : VisualAction
    {
        [SerializeField] private Book book;
        [SerializeField] private MonoBookFlipExtension extension;
    
        private bool completed;
        
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            extension.Start();
            this.book.OnFlipCompleted += OnFlipCompletedHandler;
            this.book.OnFlipStarted += OnFlipStartedHandler;
            this.book.OnPageRelease += OnPageReleaseHandler;
            await UniTask.WaitUntil(() => this.completed, PlayerLoopTiming.Update, cancellationToken);
            this.book.OnFlipCompleted -= OnFlipCompletedHandler;
            this.book.OnFlipStarted -= OnFlipStartedHandler;
            this.book.OnPageRelease -= OnPageReleaseHandler;
            this.book.interactable = false;
        }

        public override void Dispose()
        {
            base.Dispose();
            this.book.OnFlipCompleted -= OnFlipCompletedHandler;
            this.book.OnFlipStarted -= OnFlipStartedHandler;
            this.book.OnPageRelease -= OnPageReleaseHandler;
            this.book.interactable = false;
        }

        private void OnFlipCompletedHandler()
        {
            this.completed = true;
            extension.FlipCompleted();
        }

        private void OnFlipStartedHandler()
        {
            extension.FlipStart();
        }

        private void OnPageReleaseHandler()
        {
            extension.PageRelease();
        }
        
    }
}