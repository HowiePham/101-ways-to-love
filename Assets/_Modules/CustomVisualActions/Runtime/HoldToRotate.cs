using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using Mimi.VisualActions;
using UnityEngine;
using VisualActions.Areas;

public class HoldToRotate : VisualAction
{
    [SerializeField] private Transform target;
    [SerializeField] private BaseArea area;
    [SerializeField] private Vector3 targetRotation;
    [SerializeField] private float angleValue;
    [SerializeField] private float tolerance = 0.1f;
    [SerializeField, SoundKey] private string soundKey;
    [SerializeField] private BaseAudioServiceSO audioPlayer;

    private bool complete;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        try
        {
            this.complete = false;
            LeanTouch.OnFingerUpdate += FingerUpdateHandler;
            LeanTouch.OnFingerUp += FingerUpHandler;
            LeanTouch.OnFingerDown += FingerDownHandler;
            await UniTask.WaitUntil(() => this.complete, cancellationToken: cancellationToken);
            LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
            LeanTouch.OnFingerDown -= FingerDownHandler;
            await this.target.DORotate(this.targetRotation, 0.5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        }
        catch (OperationCanceledException e)
        {
        }
        finally
        {
            LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
            LeanTouch.OnFingerDown -= FingerDownHandler;
        }
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        if (finger.IsOverGui || !this.area.ContainsScreenPosition(finger.ScreenPosition, Camera.main))
        {
            return;
        }

        if (string.IsNullOrEmpty(this.soundKey))
        {
            return;
        }

        this.audioPlayer.PlaySound(this.soundKey);
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        if (finger.IsOverGui)
        {
            return;
        }

        if (!string.IsNullOrEmpty(this.soundKey))
        {
            this.audioPlayer.StopSound(this.soundKey);
        }

        Vector3 current = this.target.eulerAngles;
        bool isCorrect =
            Mathf.Abs(Mathf.DeltaAngle(current.x, this.targetRotation.x)) <= this.tolerance &&
            Mathf.Abs(Mathf.DeltaAngle(current.y, this.targetRotation.y)) <= this.tolerance &&
            Mathf.Abs(Mathf.DeltaAngle(current.z, this.targetRotation.z)) <= this.tolerance;

        if (isCorrect)
        {
            this.complete = true;
        }
    }

    private void FingerUpdateHandler(LeanFinger finger)
    {
        if (finger.IsOverGui || !this.area.ContainsScreenPosition(finger.ScreenPosition, Camera.main))
        {
            return;
        }

        float currentAngle = this.target.eulerAngles.z;
        float rotateAngle = currentAngle + this.angleValue;
        this.target.DORotate(new Vector3(0, 0, rotateAngle), 0f, RotateMode.FastBeyond360);
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
        LeanTouch.OnFingerDown -= FingerDownHandler;
    }
}