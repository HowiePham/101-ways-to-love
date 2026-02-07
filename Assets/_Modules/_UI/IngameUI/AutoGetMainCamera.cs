using System;
using UnityEngine;

public class AutoGetMainCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas levelUICanvas;

    private void Awake()
    {
        this.mainCamera = Camera.main;
        this.levelUICanvas.worldCamera = this.mainCamera;
    }
}