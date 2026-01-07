#if UNITY_EDITOR
using System.IO;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

public class DragMechanicGenerator
{
    private const string SpinePrefix = "spine";
    private const string StaticPrefix = "static";
    private const string SpineItemPrefabPath = "Assets/_Levels/_Shared/Prefabs/Item_Spine_Cat.prefab";
    private const string StaticItemPrefabPath = "Assets/_Levels/_Shared/Prefabs/Item_Static.prefab";
    private const string TapPrefabPath = "Assets/_Levels/_Shared/Prefabs/Tap_Object.prefab";
    private const string DragPrefabPath = "Assets/_Levels/_Shared/Prefabs/Drag_Object.prefab";

    public void GenerateDragMechanic(Transform playTransform, string levelPrefabPath, float psdPixelPerUnit)
    {
        var dragPhaseRoot = new GameObject("DragPhase");
        Transform dragPhaseTransform = dragPhaseRoot.transform;
        dragPhaseTransform.SetParent(playTransform);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private SkeletonDataAsset GetSpineAsset(string jsonFolderPath, string itemAssetName)
    {
        string skeletonAssetPath = Path.Combine(jsonFolderPath, itemAssetName + "_SkeletonData.asset");
        var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(skeletonAssetPath);

        if (skeletonDataAsset == null)
        {
            skeletonAssetPath = Path.Combine(jsonFolderPath, itemAssetName + ".asset");
            skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(skeletonAssetPath);
        }

        return skeletonDataAsset;
    }

    private void SetItemSpine(GameObject itemGO, SkeletonDataAsset skeletonDataAsset, Vector3 spineGraphicScale)
    {
        var skeletonAnimation = itemGO.GetComponentInChildren<SkeletonAnimation>();
        skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
        skeletonAnimation.transform.localScale = spineGraphicScale;
    }

    private void SetItemSprite(GameObject itemGO, Sprite itemSprite, int orderLayer)
    {
        var spriteRenderer = itemGO.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = itemSprite;
        spriteRenderer.sortingOrder = orderLayer;
    }

    private GameObject GetItemPrefab(string itemName)
    {
        if (itemName.Contains(SpinePrefix))
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(SpineItemPrefabPath);
        }
        else if (itemName.Contains(StaticPrefix))
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(StaticItemPrefabPath);
        }

        return null;
    }
}
#endif