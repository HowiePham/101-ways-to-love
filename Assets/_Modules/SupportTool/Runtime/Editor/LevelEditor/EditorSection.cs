using System.Collections.Generic;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

public abstract class EditorSection
{
    protected readonly LevelEditor levelEditor;

    public EditorSection(LevelEditor levelEditor)
    {
        this.levelEditor = levelEditor;
    }
    
    public abstract void DrawSection();
    
    protected void DrawObjectRenderersSection(GameObject gameObject)
    {
        Renderer renderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null)
        {
            EditorGUILayout.BeginHorizontal();
            DrawSpriteSection(renderer);
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            var skeletonAnimation = gameObject.GetComponentInChildren<SkeletonAnimation>();
            renderer = skeletonAnimation.GetComponent<MeshRenderer>();

            EditorGUILayout.BeginHorizontal();
            DrawSkeletonAnimationSection(skeletonAnimation);

            var serializedObject = new SerializedObject(skeletonAnimation);
            SerializedProperty animProp = serializedObject.FindProperty("_animationName");
            EditorGUILayout.LabelField("Action:", GUILayout.Width(50));
            EditorGUILayout.PropertyField(animProp, GUIContent.none);

            bool newLoopState = EditorGUILayout.Toggle($"Loop: ", skeletonAnimation.loop);
            if (newLoopState != skeletonAnimation.loop)
            {
                Undo.RecordObject(skeletonAnimation, "Change Loop state");
                skeletonAnimation.loop = newLoopState;
                EditorUtility.SetDirty(skeletonAnimation);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        DrawSortingLayerSection(renderer);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawOrderInLayerSection(renderer);
        EditorGUILayout.EndHorizontal();
    }
    
    protected void DrawSpriteSection(Renderer renderer)
    {
        var spriteRenderer = (SpriteRenderer)renderer;
        EditorGUILayout.LabelField("Sprite:", GUILayout.Width(60));

        var newSprite = (Sprite)EditorGUILayout.ObjectField(
            spriteRenderer.sprite,
            typeof(Sprite),
            false,
            GUILayout.Width(100),
            GUILayout.Height(100)
        );

        if (newSprite != spriteRenderer.sprite)
        {
            Undo.RecordObject(renderer, "Change Sprite");
            spriteRenderer.sprite = newSprite;
            EditorUtility.SetDirty(renderer);
        }
    }

    protected void DrawOrderInLayerSection(Renderer renderer)
    {
        EditorGUILayout.LabelField("Order In Layer:", GUILayout.Width(60));

        int newOrder = EditorGUILayout.IntField(renderer.sortingOrder);

        if (newOrder != renderer.sortingOrder)
        {
            Undo.RecordObject(renderer, "Change Order in Layer");
            renderer.sortingOrder = newOrder;
            EditorUtility.SetDirty(renderer);
        }
    }

    protected void DrawSortingLayerSection(Renderer renderer)
    {
        EditorGUILayout.LabelField("Sorting Layer:", GUILayout.Width(60));

        string[] sortingLayerNames = GetSortingLayerNames();
        int currentIndex = GetSortingLayerIndex(renderer.sortingLayerName, sortingLayerNames);

        int newIndex = EditorGUILayout.Popup(currentIndex, sortingLayerNames);

        if (newIndex != currentIndex && newIndex >= 0 && newIndex < sortingLayerNames.Length)
        {
            Undo.RecordObject(renderer, "Change Sorting Layer");
            renderer.sortingLayerName = sortingLayerNames[newIndex];
            EditorUtility.SetDirty(renderer);
        }
    }

    protected string[] GetSortingLayerNames()
    {
        System.Type internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
        System.Reflection.PropertyInfo sortingLayersProperty =
            internalEditorUtilityType.GetProperty("sortingLayerNames", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }

    protected int GetSortingLayerIndex(string layerName, string[] sortingLayerNames)
    {
        for (int i = 0; i < sortingLayerNames.Length; i++)
        {
            if (sortingLayerNames[i] == layerName)
                return i;
        }

        return 0;
    }
    
    protected void DrawSkeletonAnimationSection(SkeletonAnimation skeletonAnimation)
    {
        EditorGUILayout.LabelField("Animation:", GUILayout.Width(60));

        var newAnimation = (SkeletonDataAsset)EditorGUILayout.ObjectField(
            skeletonAnimation.SkeletonDataAsset,
            typeof(SkeletonDataAsset),
            false,
            GUILayout.Height(18)
        );

        if (newAnimation != skeletonAnimation.SkeletonDataAsset)
        {
            Undo.RecordObject(skeletonAnimation, "Change Animation");
            skeletonAnimation.skeletonDataAsset = newAnimation;
            EditorUtility.SetDirty(skeletonAnimation);
        }
    }
}