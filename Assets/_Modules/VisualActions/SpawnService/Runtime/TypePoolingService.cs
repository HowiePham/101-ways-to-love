using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mimi.SpawnService
{
    public class TypePoolingService : IPoolService
    {
        private Transform rootTf;
        private Dictionary<Type, Pool> poolInstance = new Dictionary<Type, Pool>();
        [SerializeField] private PoolAmount[] poolAmounts;

        public TypePoolingService(PoolAmount[] poolAmounts)
        {
            rootTf = new GameObject("TypeObjectPooling").transform;
            rootTf.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            UnityEngine.Object.DontDestroyOnLoad(rootTf.gameObject);
            foreach (var amount in poolAmounts)
            {
                Preload(amount.prefab, amount.amount);
            }
        }


        public void Preload(Component prefab, int amount)
        {
            if (prefab == null)
            {
                Debug.LogError(" PREFAB IN POOL IS EMPTY");
                return;
            }

            var key = prefab.GetType();
            if (!poolInstance.ContainsKey(key) || poolInstance[key] == null)
            {
                var parent = new GameObject(prefab.gameObject.name + " " + prefab.GetType()).transform;
                parent.transform.position = Vector3.zero;
                parent.SetParent(rootTf);
                Pool p = new Pool();
                p.PreLoad(prefab, amount, parent);
                poolInstance[key] = p;
            }
        }


        //Take the object to pool
        public void Despawn(Component unit)
        {
            var key = unit.GetType();
            if (!poolInstance.ContainsKey(key))
            {
                Debug.LogError(unit.name + " IS NOT PRELOAD");
            }

            poolInstance[key].Despawn(unit);
        }

        //Disable all object of type GameUnit
        public void Collect(Component unit)
        {
            var key = unit.GetType();
            if (!poolInstance.ContainsKey(key))
            {
                Debug.LogError(unit.name + "IS NOT PRELOAD !!!");
                return;
            }

            poolInstance[key].Collect();
        }


        public GameObject Spawn(Component prefab)
        {
            return Spawn<Component>(prefab).gameObject;
        }

        public GameObject Spawn(Component prefab, Vector3 position, Quaternion rotation)
        {
            var type = prefab.GetType();
            return Spawn<Component>(prefab, position, rotation).gameObject;
        }

        public T Spawn<T>(Component prefab) where T : Component
        {
            if (prefab is not T)
            {
                Debug.LogError("Spawn object is not " + typeof(T).ToString());
                return null;
            }

            var key = prefab.GetType();
            if (!poolInstance.ContainsKey(key))
            {
                Preload(prefab, 1);
            }

            return poolInstance[key].Spawn() as T;
        }

        public T Spawn<T>(Component prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            var instance = Spawn<T>(prefab);
            instance.transform.SetLocalPositionAndRotation(position, rotation);
            return instance;
        }

        public void CollectAll()
        {
            foreach (var item in poolInstance.Values)
            {
                item.Collect();
            }
        }

        //Destroy all object of type GameUnit
        public void Release(Component unit)
        {
            var key = unit.GetType();
            if (!poolInstance.ContainsKey(key))
            {
                //Debug.LogError(key.name + "IS NOT PRELOAD !!!");
                return;
            }

            poolInstance[key].Release();
        }

        public void ReleaseAll()
        {
            foreach (var item in poolInstance.Values)
            {
                item.Release();
            }
        }
    }

    public class Pool
    {
        private Transform parent;

        private Component prefab;

        //list contain unit is not using
        private Queue<Component> inactives = new Queue<Component>();

        //list contain unit is using
        private List<Component> actives = new List<Component>();

        //init pool
        public void PreLoad(Component prefab, int amount, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;

            //Debug.Log(amount);
            for (int i = 0; i < amount; i++)
            {
                Spawn(Vector3.zero, Quaternion.identity);
            }

            Collect();
        }

        //get element from pool
        public Component Spawn(Vector3 pos, Quaternion rot)
        {
            var instance = Spawn();
            instance.transform.SetPositionAndRotation(pos, rot);
            return instance;
        }

        public Component Spawn()
        {
            Component unit;
            if (inactives.Count <= 0)
            {
                unit = GameObject.Instantiate(prefab, parent);
            }
            else
            {
                unit = inactives.Dequeue();
            }

            unit.gameObject.SetActive(true);
            actives.Add(unit);
            return unit;
        }


        //return element to pool
        public void Despawn(Component unit)
        {
            if (unit != null && unit.gameObject.activeSelf)
            {
                actives.Remove(unit);
                inactives.Enqueue(unit);
                unit.gameObject.SetActive(false);
                unit.transform.SetParent(parent);
            }
        }

        //return all used element to pool
        public void Collect()
        {
            int count = actives.Count;
            for (int i = 0; i < count; i++)
            {
                Despawn(actives[0]);
            }
        
        }

        //destroy all element in pool
        public void Release()
        {
            Collect();
            while (inactives.Count > 0)
            {
                GameObject.Destroy(inactives.Dequeue().gameObject);
            }

            inactives.Clear();
        }
    }
}