using System;
using Lean.Common;
using Lean.Touch;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;
using VisualActions.Areas;

public class PlayAudioWhileInsideArea : MonoBehaviour
{
    [SerializeField] private BaseArea baseArea;
    [SerializeField] private LeanSelectable leanSelectable;

    [Header("Audio")] [SerializeField, SoundKey]
    private string soundKey;

    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Range(0f, 5f)] private float pitch = 1f;
    [SerializeField] private float delaySeconds = 0f;
    [SerializeField] private bool playSound = true;
    [SerializeField] private BaseAudioServiceSO audioPlayer;
    private bool insideArea;

    private void OnEnable()
    {
        LeanTouch.OnFingerUpdate += FingerUpdateHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        if (string.IsNullOrEmpty(this.soundKey))
        {
            return;
        }

        this.audioPlayer.StopSound(this.soundKey);
        this.insideArea = false;
    }

    private void FingerUpdateHandler(LeanFinger finger)
    {
        if (!this.leanSelectable.IsSelected || string.IsNullOrEmpty(this.soundKey))
        {
            return;
        }

        if (this.baseArea.ContainsWorldSpace(this.leanSelectable.transform.position) && !this.insideArea)
        {
            this.insideArea = true;
            this.audioPlayer.PlaySound(this.soundKey);
        }
        else if (!this.baseArea.ContainsWorldSpace(this.leanSelectable.transform.position) && this.insideArea)
        {
            this.insideArea = false;
            this.audioPlayer.StopSound(this.soundKey);
        }
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
    }
}