using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mimi.SpawnService
{
    [System.Serializable]
    public struct PoolAmount
    {
        public Component prefab;
        public int amount;
    }
}