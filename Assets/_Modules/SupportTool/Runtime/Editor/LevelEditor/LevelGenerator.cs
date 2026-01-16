using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelGenerator
{
    private const string StaticPrefix = "static";
    private const string StaticObjectParentName = "StaticObjects";
    protected const string LevelAddress = "Assets/_Levels/";

    public void GenerateStaticObjects(GameObject psd, GameObject levelRootGO)
    {
        var psdRootGO = (GameObject)PrefabUtility.InstantiatePrefab(psd);
        Transform staticObjectParent = GetTransform(levelRootGO.transform, StaticObjectParentName);
        var levelTexturesAddress = $"{LevelAddress}{levelRootGO.name}/Textures/";

        if (!File.Exists(levelTexturesAddress))
        {
            Debug.Log($"Level Folder {levelRootGO.name} does not exist");
            return;
        }

        foreach (Transform layer in psdRootGO.transform)
        {
            string objectName = layer.name.ToLower();

            if (!objectName.Contains(StaticPrefix))
            {
                continue;
            }

            GameObject objectInLevel = CreateStaticObject(layer, levelTexturesAddress);

            objectInLevel.transform.SetParent(staticObjectParent);
            objectInLevel.name = objectName;
        }

        GameObject.DestroyImmediate(psdRootGO);
    }

    private GameObject CreateStaticObject(Transform psdLayer, string textureAddress)
    {
        GameObject staticObject = Object.Instantiate(psdLayer.gameObject, psdLayer.position, psdLayer.rotation);
        var objectTextureAddress = $"{textureAddress}{psdLayer.gameObject.name}.png";
        Debug.Log(objectTextureAddress);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(objectTextureAddress);
        var newSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        var spriteRenderer = staticObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = newSprite;

        return staticObject;
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