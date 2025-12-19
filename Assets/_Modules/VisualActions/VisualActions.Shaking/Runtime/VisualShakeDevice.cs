using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MEC;
using UnityEngine;

namespace Mimi.VisualActions.Shaking
{
    public class VisualShakeDevice : VisualAction
    {
        [SerializeField] private float accelerationMagnitude = 5f;

        private bool complete;
        private CoroutineHandle shakeCoroutineHandle;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                this.complete = false;
                this.shakeCoroutineHandle = Timing.RunCoroutine(_CheckShaking());
#if UNITY_EDITOR
                this.complete = true;
#endif
                await UniTask.WaitUntil(() => this.complete, PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
            finally
            {
                if (this.shakeCoroutineHandle.IsValid)
                {
                    Timing.KillCoroutines(this.shakeCoroutineHandle);
                }
            }
        }

        private IEnumerator<float> _CheckShaking()
        {
            while (true)
            {
                if (Input.acceleration.sqrMagnitude > this.accelerationMagnitude * this.accelerationMagnitude)
                {
                    Handheld.Vibrate();
                    this.complete = true;
                    yield break;
                }

                yield return 0f;
            }
        }
    }
}