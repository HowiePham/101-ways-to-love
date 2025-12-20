#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.U2D.PSD;
using UnityEngine;

public class LevelGenPSDPostProcessor : AssetPostprocessor
{
    private const string SpinePrefix = "spine";
    private const string StaticPrefix = "static";
    private const string SequenceObjectName = "Sequence";
    private const string PlayObjectName = "Play";
    private const string StaticObjectParentName = "StaticObjects";
    private const string LevelTemplatePath = "Assets/_Levels/_Shared/Prefabs/LevelTemplate.prefab";

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (path.EndsWith(".psd"))
            {
                EditorApplication.delayCall += () => GenerateLevelFromPSD(path);
            }
        }
    }

    private static void GenerateLevelFromPSD(string psdPath)
    {
        // GenerateLevelPrefab(psdPath);
    }

    private static void GenerateLevelPrefab(string psdPath)
    {
        var psdRootGO = AssetDatabase.LoadAssetAtPath<GameObject>(psdPath);
        var psdSprite = (PSDImporter)AssetImporter.GetAtPath(psdPath);
        Debug.Log($"--- (GENERATOR) {psdRootGO.name} PPU: {psdSprite.spritePixelsPerUnit}");

        var levelTemplatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelTemplatePath);
        var levelRootGO = (GameObject)PrefabUtility.InstantiatePrefab(levelTemplatePrefab);
        Transform staticObjectParent = GetObjectParent(levelRootGO.transform, StaticObjectParentName);

        foreach (Transform child in psdRootGO.transform)
        {
            string objectName = child.name;

            if (objectName.Contains(SpinePrefix) || objectName.Contains(StaticPrefix))
            {
                continue;
            }

            GameObject objectInLevel = Object.Instantiate(child.gameObject, child.transform.position, child.transform.rotation);

            objectInLevel.transform.SetParent(staticObjectParent);
            objectInLevel.name = objectName;
        }

        GenerateLevelMechanics(psdPath, levelRootGO, psdSprite.spritePixelsPerUnit);
    }

    private static void GenerateLevelMechanics(string psdPath, GameObject levelRootGO, float psdPixelPerUnit)
    {
        string levelDataPath = Path.GetDirectoryName(psdPath);
        string levelPath = Path.GetDirectoryName(levelDataPath);

        GenerateDragMechanic(levelRootGO.transform, levelPath, psdPixelPerUnit);
    }

    private static void GenerateDragMechanic(Transform levelRoot, string levelPath, float psdPixelPerUnit)
    {
        Transform sequenceTransform = GetObjectParent(levelRoot, SequenceObjectName);
        Transform playTransform = GetObjectParent(sequenceTransform, PlayObjectName);

        string prefabPath = Path.Combine(levelPath, "Prefabs") + Path.DirectorySeparatorChar;

        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
        }

        var dragMechanicGenerator = new DragMechanicGenerator();
        dragMechanicGenerator.GenerateDragMechanic(playTransform, prefabPath, psdPixelPerUnit);
    }

    private static Transform GetObjectParent(Transform levelRoot, string parentName)
    {
        foreach (Transform child in levelRoot)
        {
            GameObject childGameObject = child.gameObject;
            if (childGameObject.name.Equals(parentName))
            {
                return child;
            }
        }

        return null;
    }
}

#endif