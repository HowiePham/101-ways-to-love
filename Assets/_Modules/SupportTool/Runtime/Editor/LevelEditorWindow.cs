using System.Collections.Generic;
using Mimi.Interactions.Dragging;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private LevelEditor levelEditor;
    private Vector2 scrollPosition;
    private bool showStaticObjects = true;
    private bool showInteractableObjects = true;
    private bool showAnimation = true;

    public void Initialize(LevelEditor editor)
    {
        this.levelEditor = editor;
    }

    public void ShowWindow()
    {
        var window = GetWindow<LevelEditorWindow>(true, "Level Editor", true);
        window.minSize = new Vector2(300, 300);
        window.maxSize = new Vector2(1000, 1200);
        window.ShowPopup();
    }

    private void OnGUI()
    {
        if (this.levelEditor == null)
        {
            EditorGUILayout.HelpBox("No Level Editor assigned!", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

        DrawStaticObjectsSection();

        EditorGUILayout.Space(20);

        DrawInteractableObjectSection();

        EditorGUILayout.Space(20);

        DrawAnimationSection();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space(10);
        DrawBottomButtons();
    }

    private void DrawAnimationSection()
    {
        this.showAnimation = EditorGUILayout.Foldout(this.showAnimation, "Animation", true, EditorStyles.foldoutHeader);
        if (!this.showAnimation)
        {
            return;
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        var newAnimation = (SkeletonDataAsset)EditorGUILayout.ObjectField(
            this.levelEditor.SkeletonAnimation.SkeletonDataAsset,
            typeof(SkeletonDataAsset),
            false,
            GUILayout.Height(18)
        );

        if (newAnimation != this.levelEditor.SkeletonAnimation.SkeletonDataAsset)
        {
            Undo.RecordObject(this.levelEditor.SkeletonAnimation.SkeletonDataAsset, "Change Animation");
            this.levelEditor.SkeletonAnimation.skeletonDataAsset = newAnimation;
            EditorUtility.SetDirty(this.levelEditor.SkeletonAnimation.skeletonDataAsset);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawInteractableObjectSection()
    {
        this.showInteractableObjects = EditorGUILayout.Foldout(this.showInteractableObjects, "Interactable Objects", true, EditorStyles.foldoutHeader);
        if (!this.showInteractableObjects)
        {
            return;
        }

        BaseDraggable[] interactableObjects = this.levelEditor.BaseDraggables;
        List<SpriteRenderer> renderers = this.levelEditor.InteractableObjectRenderers;

        if (renderers == null || renderers.Count == 0)
        {
            EditorGUILayout.HelpBox("No objects found", MessageType.Info);
            return;
        }

        EditorGUI.indentLevel++;

        for (int i = 0; i < renderers.Count; i++)
        {
            SpriteRenderer renderer = renderers[i];
            BaseDraggable interactableObject = interactableObjects[i];
            if (renderer == null) continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{i + 1}. ", EditorStyles.boldLabel, GUILayout.Width(25));
            string newName = EditorGUILayout.TextField(interactableObject.gameObject.name);

            if (!newName.Equals(renderer.gameObject.name))
            {
                Undo.RecordObject(interactableObject.gameObject, "Change Name");
                interactableObject.gameObject.name = newName;
                EditorUtility.SetDirty(interactableObject.gameObject);
            }

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = interactableObject.gameObject;
                EditorGUIUtility.PingObject(interactableObject.gameObject);
            }

            EditorGUILayout.EndHorizontal();

            DrawObjectRenderersSection(renderer);
        }

        EditorGUI.indentLevel--;
    }

    private void DrawStaticObjectsSection()
    {
        this.showStaticObjects = EditorGUILayout.Foldout(this.showStaticObjects, "Static Objects", true, EditorStyles.foldoutHeader);
        if (!this.showStaticObjects)
        {
            return;
        }

        SpriteRenderer[] renderers = this.levelEditor.StaticObjectRenderers;
        if (renderers == null || renderers.Length == 0)
        {
            EditorGUILayout.HelpBox("No objects found", MessageType.Info);
            return;
        }

        EditorGUI.indentLevel++;

        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null) continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{i + 1}. ", EditorStyles.boldLabel, GUILayout.Width(25));
            string newName = EditorGUILayout.TextField(renderer.gameObject.name);

            if (!newName.Equals(renderer.gameObject.name))
            {
                Undo.RecordObject(renderer.gameObject, "Change Name");
                renderer.gameObject.name = newName;
                EditorUtility.SetDirty(renderer.gameObject);
            }

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = renderer.gameObject;
                EditorGUIUtility.PingObject(renderer.gameObject);
            }

            EditorGUILayout.EndHorizontal();

            DrawObjectRenderersSection(renderer);
        }

        EditorGUI.indentLevel--;
    }

    private void DrawObjectRenderersSection(SpriteRenderer renderer)
    {
        EditorGUILayout.BeginHorizontal();
        DrawSpriteSection(renderer);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawSortingLayerSection(renderer);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawOrderInLayerSection(renderer);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawSpriteSection(SpriteRenderer renderer)
    {
        EditorGUILayout.LabelField("Sprite:", GUILayout.Width(60));

        var newSprite = (Sprite)EditorGUILayout.ObjectField(
            renderer.sprite,
            typeof(Sprite),
            false,
            GUILayout.Height(18)
        );

        if (newSprite != renderer.sprite)
        {
            Undo.RecordObject(renderer, "Change Sprite");
            renderer.sprite = newSprite;
            EditorUtility.SetDirty(renderer);
        }
    }

    private void DrawOrderInLayerSection(SpriteRenderer renderer)
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

    private void DrawSortingLayerSection(SpriteRenderer renderer)
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

    private string[] GetSortingLayerNames()
    {
        System.Type internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
        System.Reflection.PropertyInfo sortingLayersProperty =
            internalEditorUtilityType.GetProperty("sortingLayerNames", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }

    private int GetSortingLayerIndex(string layerName, string[] sortingLayerNames)
    {
        for (int i = 0; i < sortingLayerNames.Length; i++)
        {
            if (sortingLayerNames[i] == layerName)
                return i;
        }

        return 0;
    }

    private void DrawBottomButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Refresh Data", GUILayout.Height(30)))
        {
            this.levelEditor.PrepareData();
            Repaint();
        }

        if (GUILayout.Button("Save Changes", GUILayout.Height(30)))
        {
            EditorUtility.SetDirty(this.levelEditor.gameObject);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", "Changes saved!", "OK");
        }

        EditorGUILayout.EndHorizontal();
    }
}