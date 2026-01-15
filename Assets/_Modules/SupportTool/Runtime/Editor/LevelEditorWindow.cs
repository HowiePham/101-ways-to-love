using System.Collections.Generic;
using Mimi.Actor.Graphic.Core;
using Mimi.Interactions.Dragging;
using Mimi.VisualActions.Spines;
using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VisualActions.Areas;

public class LevelEditorWindow : EditorWindow
{
    private LevelEditor levelEditor;
    private Vector2 scrollPosition;
    private bool showStaticObjects = true;
    private bool showInteractableObjects = true;
    private bool showInteractingBoxes = true;
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
        DrawInteractingBoxSection();
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

        SkeletonAnimation skeletonAnimation = this.levelEditor.SkeletonAnimation;
        DrawSkeletonAnimationSection(skeletonAnimation);

        if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
        {
            Selection.activeGameObject = skeletonAnimation.gameObject;
            EditorGUIUtility.PingObject(skeletonAnimation.gameObject);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Order In Layer:", GUILayout.Width(60));

        var renderer = skeletonAnimation.GetComponent<MeshRenderer>();
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
                true,
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

    private void DrawInteractingBoxSection()
    {
        this.showInteractingBoxes = EditorGUILayout.Foldout(this.showInteractingBoxes, "Interacting Boxes", true, EditorStyles.foldoutHeader);
        if (!this.showInteractingBoxes)
        {
            return;
        }

        InteractingBox[] interactingBoxes = this.levelEditor.InteractingBoxes;
        if (interactingBoxes == null || interactingBoxes.Length == 0)
        {
            EditorGUILayout.HelpBox("No boxes found", MessageType.Info);
            return;
        }

        EditorGUI.indentLevel++;

        for (var i = 0; i < interactingBoxes.Length; i++)
        {
            InteractingBox interactingBox = interactingBoxes[i];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i + 1}. {interactingBox.gameObject.name}", EditorStyles.boldLabel);

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = interactingBox.gameObject;
                EditorGUIUtility.PingObject(interactingBox.gameObject);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            bool followTarget = EditorGUILayout.Toggle($"Box Following Object ", interactingBox.FollowTarget);
            if (followTarget != interactingBox.FollowTarget)
            {
                Undo.RecordObject(interactingBox, "Change Following toggle");
                interactingBox.FollowTarget = followTarget;
                EditorUtility.SetDirty(interactingBox);
            }

            if (!interactingBox.FollowTarget)
            {
                EditorGUILayout.EndHorizontal();

                var boxArea = interactingBox.gameObject.GetComponent<BoxArea>();
                if (boxArea != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    Vector2 newSize = EditorGUILayout.Vector2Field("Box Size:", boxArea.Size);

                    if (newSize != boxArea.Size)
                    {
                        Undo.RecordObject(boxArea, "Change Box Size");
                        boxArea.Size = newSize;
                        EditorUtility.SetDirty(boxArea);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(10);

                continue;
            }

            var target = (Transform)EditorGUILayout.ObjectField(
                interactingBox.Target,
                typeof(Transform),
                true,
                GUILayout.Height(18)
            );

            if (target != interactingBox.Target)
            {
                Undo.RecordObject(interactingBox, "Change Target transform");
                interactingBox.Target = target;
                EditorUtility.SetDirty(interactingBox);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
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

        if (interactableObjects == null || interactableObjects.Count == 0)
        {
            EditorGUILayout.HelpBox("No objects found", MessageType.Info);
            return;
        }

        EditorGUI.indentLevel++;

        for (int i = 0; i < interactableObjects.Count; i++)
        {
            GameObject interactableObject = interactableObjects[i];
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{i + 1}. ", EditorStyles.boldLabel, GUILayout.Width(25));
            string newName = EditorGUILayout.TextField(interactableObject.name);

            if (!newName.Equals(interactableObject.name))
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

            var boxArea = interactableObject.GetComponent<BoxArea>();
            if (boxArea != null)
            {
                EditorGUILayout.BeginHorizontal();
                Vector2 newSize = EditorGUILayout.Vector2Field("Box Size:", boxArea.Size);

                if (newSize != boxArea.Size)
                {
                    Undo.RecordObject(boxArea, "Change Box Size");
                    boxArea.Size = newSize;
                    EditorUtility.SetDirty(boxArea);
                }

                EditorGUILayout.EndHorizontal();
            }

            DrawObjectRenderersSection(interactableObject);

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

        List<GameObject> staticObjects = this.levelEditor.StaticObjects;
        if (staticObjects == null || staticObjects.Count == 0)
        {
            EditorGUILayout.HelpBox("No objects found", MessageType.Info);
            return;
        }

        EditorGUI.indentLevel++;

        for (int i = 0; i < staticObjects.Count; i++)
        {
            GameObject staticObject = staticObjects[i];
            if (staticObject == null) continue;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{i + 1}. ", EditorStyles.boldLabel, GUILayout.Width(25));
            string newName = EditorGUILayout.TextField(staticObject.gameObject.name);

            if (!newName.Equals(staticObject.gameObject.name))
            {
                Undo.RecordObject(staticObject.gameObject, "Change Name");
                staticObject.gameObject.name = newName;
                EditorUtility.SetDirty(staticObject.gameObject);
            }

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = staticObject.gameObject;
                EditorGUIUtility.PingObject(staticObject.gameObject);
            }

            EditorGUILayout.EndHorizontal();

            DrawObjectRenderersSection(staticObject);

            EditorGUILayout.Space(10);
        }

        EditorGUI.indentLevel--;
    }

    private void DrawObjectRenderersSection(GameObject gameObject)
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
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        DrawSortingLayerSection(renderer);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawOrderInLayerSection(renderer);
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawSkeletonAnimationSection(SkeletonAnimation skeletonAnimation)
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

    private void DrawSpriteSection(Renderer renderer)
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

        if (GUILayout.Button("Generate Level Sound", GUILayout.Height(30)))
        {
            var creatingPlaySoundSpine = FindAnyObjectByType<CreatePlaySoundSpine>();

            if (creatingPlaySoundSpine == null)
            {
                Debug.LogError($"Do not have any Sound Spine Generator!");
            }
            else
            {
                creatingPlaySoundSpine.Generate();
            }
        }

        EditorGUILayout.EndHorizontal();
    }
}