using System;
using Cysharp.Threading.Tasks;
using Mimi.Ads.Adapters;
using UnityEngine;

namespace _Modules.Ads
{
    public class TimeoutAdAdapter : AdapterDecorator
    {
        private readonly float timeoutSeconds;

        public TimeoutAdAdapter(IAdAdapter inner, float timeoutSeconds = 5f) : base(inner)
        {
            this.timeoutSeconds = timeoutSeconds;
        }

        public override async UniTask Initialize()
        {
            var cts = new System.Threading.CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(this.timeoutSeconds));
            try
            {
                await base.Initialize().AttachExternalCancellation(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[Ads] SDK init timed out after {this.timeoutSeconds}s — continuing without full init");
            }
            finally
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}
