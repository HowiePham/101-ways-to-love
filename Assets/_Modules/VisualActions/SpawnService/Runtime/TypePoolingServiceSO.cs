using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mimi.SpawnService
{
    [CreateAssetMenu(menuName = "ServicesSO/Pooling/TypePoolingService")]
    public class TypePoolingServiceSO : BasePoolServiceSO
    {
        public override IPoolService CreatePoolService()
        {
            return new TypePoolingService(poolAmounts);
        }
    }
}