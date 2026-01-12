using System.Collections.Generic;
using Mimi.Interactions.Dragging;
using Mimi.VisualActions.Spines;
using Spine.Unity;
using Spine.Unity.Editor;
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

        EditorGUILayout.BeginVertical("box");
        DrawStaticObjectsSection();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginVertical("box");
        DrawInteractableObjectSection();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginVertical("box");
        DrawAnimationSection();
        EditorGUILayout.EndVertical();

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

        EditorGUILayout.BeginHorizontal();

        SkeletonAnimation skeletionAnimation = this.levelEditor.SkeletonAnimation;
        var newAnimation = (SkeletonDataAsset)EditorGUILayout.ObjectField(
            skeletionAnimation.SkeletonDataAsset,
            typeof(SkeletonDataAsset),
            false,
            GUILayout.Height(18)
        );

        if (newAnimation != skeletionAnimation.SkeletonDataAsset)
        {
            Undo.RecordObject(skeletionAnimation, "Change Animation");
            skeletionAnimation.skeletonDataAsset = newAnimation;
            EditorUtility.SetDirty(skeletionAnimation);
        }

        if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
        {
            Selection.activeGameObject = skeletionAnimation.gameObject;
            EditorGUIUtility.PingObject(skeletionAnimation.gameObject);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Order In Layer:", GUILayout.Width(60));

        var renderer = skeletionAnimation.GetComponent<MeshRenderer>();
        EditorGUILayout.BeginHorizontal();
        DrawSortingLayerSection(renderer);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawOrderInLayerSection(renderer);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        SpineAnimMechanic[] spineAnimMechanics = this.levelEditor.SpineAnimMechanics;
        EditorGUI.indentLevel++;

        for (int i = 0; i < spineAnimMechanics.Length; i++)
        {
            SpineAnimMechanic animMechanic = spineAnimMechanics[i];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i + 1}. {animMechanic.gameObject.name}", EditorStyles.boldLabel);

            var newAnim = (SkeletonAnimation)EditorGUILayout.ObjectField(
                animMechanic.SkeletonAnimation,
                typeof(SkeletonAnimation),
                false,
                GUILayout.Height(18)
            );

            if (newAnim != animMechanic.SkeletonAnimation)
            {
                Undo.RecordObject(animMechanic, "Change Spine Animation");
                animMechanic.SkeletonAnimation = newAnim;
                EditorUtility.SetDirty(animMechanic);
            }

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = animMechanic.gameObject;
                EditorGUIUtility.PingObject(animMechanic.gameObject);
            }

            EditorGUILayout.EndHorizontal();

            var serializedObject = new SerializedObject(animMechanic);
            SerializedProperty animProp = serializedObject.FindProperty("animation");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Action:", GUILayout.Width(50));
            EditorGUILayout.PropertyField(animProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.indentLevel--;
    }

    private void DrawInteractableObjectSection()
    {
        this.showInteractableObjects = EditorGUILayout.Foldout(this.showInteractableObjects, "Interactable Objects", true, EditorStyles.foldoutHeader);
        if (!this.showInteractableObjects)
        {
            return;
        }

        List<GameObject> interactableObjects = this.levelEditor.InteractableObjects;
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
            GameObject interactableObject = interactableObjects[i];
            if (renderer == null) continue;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{i + 1}. ", EditorStyles.boldLabel, GUILayout.Width(25));
            string newName = EditorGUILayout.TextField(interactableObject.name);

            if (!newName.Equals(renderer.gameObject.name))
            {
                Undo.RecordObject(interactableObject, "Change Name");
                interactableObject.name = newName;
                EditorUtility.SetDirty(interactableObject);
            }

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = interactableObject;
                EditorGUIUtility.PingObject(interactableObject);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            var objectHighlight = interactableObject.GetComponent<ScaleObjectHighlight>();
            var highlightToggle = EditorGUILayout.Toggle($"Highlighted Object ", objectHighlight.EnableHighlight);

            if (highlightToggle != objectHighlight.EnableHighlight)
            {
                Undo.RecordObject(objectHighlight, "Change Highlight toggle");
                objectHighlight.EnableHighlight = highlightToggle;
                EditorUtility.SetDirty(objectHighlight);
            }

            EditorGUILayout.EndHorizontal();

            DrawObjectRenderersSection(renderer);

            EditorGUILayout.Space(10);
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

            EditorGUILayout.Space(10);
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

    private void DrawOrderInLayerSection(Renderer renderer)
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

    private void DrawSortingLayerSection(Renderer renderer)
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

        EditorGUILayout.EndHorizontal();
    }
}