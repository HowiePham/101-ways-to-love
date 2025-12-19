using Mimi.VisualActions.Attribute;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace VisualActions.VisualActions.GameObjects.Runtime
{
    public class FetchGameObjectDependency : MonoBehaviour
    {
        [SerializeField]
        private GameObject input;

        [SerializeField] private GameObject fetchedGO;
        
        #if UNITY_EDITOR

        [Button]
        public void FetchDependency()
        {
            if (input != null && fetchedGO != null)
            {
                fetchedGO.FetchDependency(input);
                foreach (var component in fetchedGO.GetComponentsInChildren<MonoBehaviour>())
                {
                    EditorUtility.SetDirty(component);
                }
            }
            
        }
        #endif
    }
}