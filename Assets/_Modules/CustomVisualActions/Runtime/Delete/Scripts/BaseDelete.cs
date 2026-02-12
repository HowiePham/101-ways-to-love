using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

public interface IBaseDelete
{
    public event Action<Vector3> OnStartDelete;
    public event Action OnStopDelete;
    public event Action<Vector3> OnDeleting;

    public void FingerDownHandler(LeanFinger finger);

    public void FingerUpdateHandler(LeanFinger finger);

    public void FingerUpHandler(LeanFinger finger);
}
