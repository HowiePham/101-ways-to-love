using UnityEngine;

namespace Mimi.SpawnService
{
    public interface IPoolService 
    {
        void Preload(Component prefab, int amount);
        GameObject Spawn(Component prefab);
        GameObject Spawn(Component prefab, Vector3 position, Quaternion rotation);
        T Spawn<T>(Component prefab) where T : Component;
        T Spawn<T>(Component prefab, Vector3 position, Quaternion rotation) where T : Component;
        void Despawn(Component go);
        void Collect(Component prefab);
        void CollectAll();
        void Release(Component go);
        void ReleaseAll();
    }
}