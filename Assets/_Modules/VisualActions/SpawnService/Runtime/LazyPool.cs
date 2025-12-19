using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mimi.SpawnService
{
    [Serializable]
    struct GameObjectPool
    {
        public GameObject objectToSpawn;
        public int numberToSpawn;
    }

    public class LazyPool : MonoBehaviour,IPoolService
    {
        [SerializeField] private List<GameObjectPool> listGameObjectToPreload;
        private Dictionary<GameObject, List<GameObject>> _poolObj = new Dictionary<GameObject, List<GameObject>>();
        private IPoolService _poolServiceImplementation;

        protected void Awake()
        {
            PreLoadObject();
        }

        private void PreLoadObject()
        {
            if (listGameObjectToPreload == null) return;
            foreach (var obj in listGameObjectToPreload)
            {
                Preload(obj.objectToSpawn.transform, obj.numberToSpawn);
            }
        }
        
        public void Preload(Component prefab, int amount)
        {
            var key = prefab.gameObject;
            if (_poolObj.ContainsKey(key)) return;
            _poolObj.Add(key, new List<GameObject>());
            for (int i = 0; i < amount; i++)
            {
                GameObject gameObjInstance = Instantiate(key, transform);
                _poolObj[key].Add(gameObjInstance);
                gameObjInstance.SetActive(false);
            }
        }

        public GameObject Spawn(Component prefab)
        {
            var key = prefab.gameObject;
            if (!_poolObj.ContainsKey(key))
            {
                _poolObj.Add(key, new List<GameObject>());
            }

            foreach (GameObject g in _poolObj[key])
            {
                if (g.activeSelf || g.transform.parent != transform)
                    continue;
                g.SetActive(true);
                return g;
            }

            GameObject g2 = Instantiate(key, transform);
            _poolObj[key].Add(g2);
            return g2;
        }

        public GameObject Spawn(Component prefab, Vector3 position, Quaternion rotation)
        {
            var instance = Spawn(prefab);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public T Spawn<T>(Component prefab) where T : Component
        {
            return Spawn(prefab).GetComponent<T>();
        }

        public T Spawn<T>(Component prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab, position, rotation).GetComponent<T>();
        }

        public void Despawn(Component go)
        {
            go.gameObject.SetActive(false);
            go.transform.SetParent(transform);
        }

        public void Collect(Component prefab)
        {
            if (!_poolObj.ContainsKey(prefab.gameObject))
            {
                Debug.LogError($"Lazy Pool not contain key {prefab.name}");
                return;
            }

            foreach (var obj in _poolObj[prefab.gameObject])
            {
                Despawn(obj.transform);
            }
        }

        public void CollectAll()
        {
            foreach (var key in _poolObj.Keys)
            {
                Collect(key.transform);
            }
        }
        

        public void Release(Component objectPrefab)
        {
            if (!_poolObj.ContainsKey(objectPrefab.gameObject))
            {
                return;
            }

            foreach (var obj in _poolObj[objectPrefab.gameObject])
            {
                Destroy(obj);
            }

            _poolObj[objectPrefab.gameObject].Clear();
            _poolObj.Remove(objectPrefab.gameObject);
        }

        public void ReleaseAll()
        {
            foreach (var key in _poolObj.Keys)
            {
                foreach (var obj in _poolObj[key])
                {
                    Destroy(obj);
                }
            }
            _poolObj.Clear();
        }
        
    }
}