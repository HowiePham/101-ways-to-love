using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

namespace Drag.Sample
{
    public class TestExecute : MonoBehaviour
    {
        [SerializeField] private VisualAction action;

        private CancellationTokenSource cancellationTokenSource;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        async UniTask Start()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            await action.Initialize();
            await action.Execute(this.cancellationTokenSource.Token);
        }

        private void OnDisable()
        {
            this.cancellationTokenSource.Cancel();
        }
    }
}