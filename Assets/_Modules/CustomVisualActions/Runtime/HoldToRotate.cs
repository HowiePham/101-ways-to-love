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
    private bool isFingerInArea;
    private int trackedFingerIndex = -1;
    private Vector2 prevFingerScreenPosition;

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

        if (this.trackedFingerIndex != -1)
        {
            return;
        }

        this.isFingerInArea = true;
        this.trackedFingerIndex = finger.Index;
        this.prevFingerScreenPosition = finger.ScreenPosition;

        if (string.IsNullOrEmpty(this.soundKey))
        {
            return;
        }

        this.audioPlayer.PlaySound(this.soundKey);
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        if (finger.IsOverGui || finger.Index != this.trackedFingerIndex)
        {
            return;
        }

        this.isFingerInArea = false;
        this.trackedFingerIndex = -1;

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
        if (finger.IsOverGui || !this.isFingerInArea || finger.Index != this.trackedFingerIndex)
        {
            return;
        }

        Vector2 currentScreenPos = finger.ScreenPosition;
        Vector2 pivotScreenPos = Camera.main.WorldToScreenPoint(this.target.position);

        float prevAngle = Vector2.SignedAngle(Vector2.up, this.prevFingerScreenPosition - pivotScreenPos);
        float currentAngle = Vector2.SignedAngle(Vector2.up, currentScreenPos - pivotScreenPos);
        float angleDelta = Mathf.DeltaAngle(prevAngle, currentAngle);

        Vector3 euler = this.target.eulerAngles;
        euler.z += angleDelta * this.angleValue;
        this.target.eulerAngles = euler;

        this.prevFingerScreenPosition = currentScreenPos;
    }

    private void OnDisable()
    {
        this.isFingerInArea = false;
        this.trackedFingerIndex = -1;
        LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
        LeanTouch.OnFingerDown -= FingerDownHandler;
    }
}