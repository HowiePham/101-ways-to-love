using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualActions.Areas;

namespace _Modules.VisualFlow.Mechanics.FindSortMatch
{
    public class TapToRotate : VisualAction
    {
        [SerializeField, InlineEditor(InlineEditorModes.FullEditor), Required]
        private BaseArea tapArea;

        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private Transform rotateObject;
        [SerializeField] private float angleValue = 90;
        [SerializeField] private float tweenDuration;

        [Header("SFX")] [SerializeField] protected BaseAudioServiceSO audioService;
        [SerializeField, SoundKey] private string soundKey;

        private bool complete;
        private bool isRotate;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                this.complete = false;
                LeanTouch.OnFingerTap += FingerTapHandler;
                await UniTask.WaitUntil(() => this.complete, PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
            finally
            {
                LeanTouch.OnFingerTap -= FingerTapHandler;
            }
        }

        private void FingerTapHandler(LeanFinger finger)
        {
            if (finger.IsOverGui) return;
            if (!this.tapArea.ContainsScreenPosition(finger.ScreenPosition, Camera.main)) return;
            if (this.isRotate == false)
            {
                RotateEffect();
            }
        }

        private async UniTask RotateEffect()
        {
            if (!string.IsNullOrEmpty(this.soundKey))
            {
                this.audioService.StopSound(this.soundKey);
                this.audioService.PlaySound(this.soundKey);
            }

            this.isRotate = true;
            var currentAngle = this.rotateObject.eulerAngles.z;
            var rotateAngle = currentAngle - this.angleValue;
            await this.rotateObject.DORotate(new Vector3(0, 0, rotateAngle), this.tweenDuration).SetEase(ease).AsyncWaitForCompletion();
            this.isRotate = false;
        }
    }
}