using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

[ExecuteInEditMode]
public class MoveWinCamera : VisualAction
{
    [SerializeField] private Transform winCameraDestination;

    private void LateUpdate()
    {
        if (Application.isPlaying || this.winCameraDestination == null)
        {
            return;
        }

        GameObject winCamera = GameObject.Find("WinCamera");
        if (winCamera == null)
        {
            return;
        }

        this.winCameraDestination.position = winCamera.transform.position;
    }

    private void UpdateCameraPosition()
    {
        GameObject winCamera = GameObject.Find("WinCamera");
        if (winCamera == null || this.winCameraDestination == null)
        {
            return;
        }

        winCamera.transform.position = this.winCameraDestination.position;
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        UpdateCameraPosition();
        await UniTask.CompletedTask;
    }
}