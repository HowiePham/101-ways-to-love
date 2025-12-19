using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor
{
    public abstract class BaseGenerateObject<T> : IGenerateObject where T : Object
    {
        public abstract string PrefabAddress { get; }

        public T GeneratePrefab()
        {
            var address = ConfigGenerateSO.Instance.rootPath + PrefabAddress;
            T prefab = AssetDatabase.LoadAssetAtPath<T>(address);
            if (prefab == null)
            {
                Debug.LogError($"address {address} khong dung prefab");
                return null;
            }

            return (T)PrefabUtility.InstantiatePrefab(prefab);
        }
    }
}