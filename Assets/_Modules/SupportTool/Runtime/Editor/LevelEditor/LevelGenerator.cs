using UnityEditor;
using UnityEngine;

public class LevelGenerator
{
    private const string StaticPrefix = "static";
    private const string SequenceObjectName = "RootSequence";
    private const string StaticObjectParentName = "StaticObjects";

    public void GenerateStaticObjects(GameObject psd, GameObject levelRootGO)
    {
        var psdRootGO = (GameObject)PrefabUtility.InstantiatePrefab(psd);
        Transform staticObjectParent = GetTransform(levelRootGO.transform, StaticObjectParentName);

        foreach (Transform child in psdRootGO.transform)
        {
            string objectName = child.name.ToLower();

            if (!objectName.Contains(StaticPrefix))
            {
                continue;
            }

            GameObject objectInLevel = Object.Instantiate(child.gameObject, child.transform.position, child.transform.rotation);

            objectInLevel.transform.SetParent(staticObjectParent);
            objectInLevel.name = objectName;
        }

        GameObject.DestroyImmediate(psdRootGO);
    }

    private Transform GetTransform(Transform levelRoot, string name)
    {
        foreach (Transform child in levelRoot)
        {
            GameObject childGameObject = child.gameObject;
            if (childGameObject.name.Equals(name))
            {
                return child;
            }
        }

        return null;
    }
}