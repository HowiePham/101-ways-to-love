using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using Mimi.VisualActions.DragHold;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    [TypeInfoBox("Updates sprite alpha based on progress.")]
    public class UpdateSpriteAlphaProgress : BaseUpdateProgress
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float targetAlpha;
        [SerializeField] private float speed = 1f;

        private Coroutine updateCoroutine;

        /// <summary>
        /// Updates sprite alpha during progress.
        /// </summary>
        /// <param name="progress">The current progress value.</param>
        /// <param name="finger">The LeanFinger input data.</param>
        /// <param name="isFingerDown">Whether the finger is down.</param>
        /// <param name="isIncreaseTime">Whether time is increasing.</param>
        public override void OnUpdateProgress(float progress, LeanFinger finger, bool isFingerDown, bool isIncreaseTime)
        {
            if (!isIncreaseTime) return;
            var currentColor = spriteRenderer.color;
            currentColor.a = Mathf.Lerp(currentColor.a, progress * targetAlpha, Time.deltaTime * speed);
            spriteRenderer.color = currentColor;

        }

        /// <summary>
        /// Sets sprite alpha to target on completion.
        /// </summary>
        public override void CompleteProgress()
        {
            var currentColor = spriteRenderer.color;
            currentColor.a = targetAlpha;
            spriteRenderer.color = currentColor;
        }

        /// <summary>
        /// Initializes the progress updater.
        /// </summary>
        public override void OnInit()
        {

        }

        /// <summary>
        /// Sets the target alpha value.
        /// </summary>
        /// <param name="alpha">The target alpha to set.</param>
        public void SetTargetAlpha(float alpha)
        {
            this.targetAlpha = alpha;
        }

        /// <summary>
        /// Sets the sprite renderer.
        /// </summary>
        /// <param name="spriteRenderer">The SpriteRenderer to set.</param>
        public void SetSpriteRenderer(SpriteRenderer spriteRenderer)
        {
            this.spriteRenderer = spriteRenderer;
        }
    }
}