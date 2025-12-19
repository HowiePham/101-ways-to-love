
using Cysharp.Threading.Tasks;
using Mimi.Services.ScriptableObject.Core;
using UnityEngine;

namespace Mimi.SpawnService
{
    public abstract class BasePoolServiceSO : BaseServiceSO, IPoolService
    {
        [SerializeField] protected PoolAmount[] poolAmounts;
        private IPoolService wrapPoolService;

        public IPoolService WrapPoolService
        {
            get
            {
                if (wrapPoolService == null)
                {
                    Initialize();
                }
                return wrapPoolService;
            }
        }

        public abstract IPoolService CreatePoolService();
        public override UniTask Initialize()
        {
            wrapPoolService = CreatePoolService();
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            WrapPoolService.ReleaseAll();
        }

        public void Preload(Component prefab, int amount)
        {
            WrapPoolService.Preload(prefab, amount);
        }

        public GameObject Spawn(Component prefab)
        {
            return WrapPoolService.Spawn(prefab);
        }

        public GameObject Spawn(Component prefab, Vector3 position, Quaternion rotation)
        {
            return WrapPoolService.Spawn(prefab, position, rotation);
        }

        public T Spawn<T>(Component prefab) where T : Component
        {
            return WrapPoolService.Spawn<T>(prefab);
        }

        public T Spawn<T>(Component prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return WrapPoolService.Spawn<T>(prefab, position, rotation);
        }

        public void Despawn(Component go)
        {
            WrapPoolService.Despawn(go);
        }

        public void Collect(Component prefab)
        {
            WrapPoolService.Collect(prefab);
        }

        public void CollectAll()
        {
            WrapPoolService.CollectAll();
        }

        public void Release(Component go)
        {
            WrapPoolService.Release(go);
        }

        public void ReleaseAll()
        {
            WrapPoolService.ReleaseAll();
        }
    }
}