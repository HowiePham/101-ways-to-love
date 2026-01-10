using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.LevelManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LeveLoaders
{
    public class AddressableLevelLoader : ILevelLoader
    {
        private readonly ILevelRepository levelRepository;
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> levelLoaders;

        public AddressableLevelLoader(ILevelRepository levelRepository)
        {
            this.levelRepository = levelRepository;
            this.levelLoaders = new Dictionary<string, AsyncOperationHandle<GameObject>>(2);
        }

        public async UniTask<GameObject> Load(string levelId)
        {
            this.levelRepository.TryGet(levelId, out LevelInfo level);
            
            if (this.levelLoaders.ContainsKey(level.PrefabAddress))
            {
                return this.levelLoaders[level.PrefabAddress].Result;
            }

            AsyncOperationHandle<GameObject> loader = Addressables.LoadAssetAsync<GameObject>(level.PrefabAddress);
            GameObject prefab = await loader.Task.AsUniTask();
            this.levelLoaders.Add(level.PrefabAddress, loader);
            return prefab;
        }

        public void Release(string levelId)
        {
            this.levelRepository.TryGet(levelId, out LevelInfo level);
            if (this.levelLoaders.ContainsKey(level.PrefabAddress))
            {
                Addressables.Release(this.levelLoaders[level.PrefabAddress]);
                this.levelLoaders.Remove(level.PrefabAddress);
            }
        }

        public void ReleaseAllExcept(string levelId)
        {
            this.levelRepository.TryGet(levelId, out LevelInfo level);

            foreach (string prefabAddress in this.levelLoaders.Keys.ToArray())
            {
                if (level.PrefabAddress == prefabAddress) continue;
                Addressables.Release(this.levelLoaders[prefabAddress]);
                this.levelLoaders.Remove(prefabAddress);
            }
        }

        public void ReleaseAll()
        {
            foreach (string key in this.levelLoaders.Keys)
            {
                Addressables.Release(this.levelLoaders[key]);
            }

            this.levelLoaders.Clear();
        }
    }
}