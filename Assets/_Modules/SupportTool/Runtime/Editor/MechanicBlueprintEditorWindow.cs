using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Dragging;
using Mimi.VisualActions.Spines;
using Mimi.VisualActions.Tapping;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;
using VisualActions.VisualActions.GameObjects.Runtime;

public class MechanicBlueprintEditorWindow : EditorWindow
{
    private const string DragBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Drag/";
    private const string DraggableObjectBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Drag/Draggable_Object.prefab";
    private const string TapBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Tap/";
    private string objectName = "";

    public static void ShowWindow()
    {
        var window = GetWindow<MechanicBlueprintEditorWindow>(true, "Select Mechanic Blueprint", true);
        window.minSize = new Vector2(500, 300);
        window.maxSize = new Vector2(5000, 3000);
        window.ShowPopup();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Object (Ex: Cancau)", EditorStyles.boldLabel);
        this.objectName = EditorGUILayout.TextField(this.objectName);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Choose Mechanic Blueprint:", EditorStyles.boldLabel);

        DrawButton("Drag_True", () => OnMenuItemClicked("Drag_True"));
        DrawButton("Drag_False", () => OnMenuItemClicked("Drag_False"));
        DrawButton("Tap_True", () => OnMenuItemClicked("Tap_True"));
        DrawButton("Tap_False", () => OnMenuItemClicked("Tap_False"));
        DrawButton("Timing", () => OnMenuItemClicked("Timing"));
    }

    private void DrawButton(string label, System.Action onClick)
    {
        bool clicked = GUILayout.Button(label);

        if (clicked)
        {
            onClick?.Invoke();
            Close();
        }
    }

    private void OnMenuItemClicked(string menuName)
    {
        if (string.IsNullOrEmpty(this.objectName))
        {
            Debug.LogError($"You have to name this interacable object");
            return;
        }

        string blueprintAddress = GetBlueprintAddress(menuName);
        var blueprintTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{blueprintAddress}{menuName}.prefab");

        if (blueprintTemplate == null)
        {
            Debug.LogError($"There is no Blueprint: {menuName}");
            return;
        }

        Debug.Log($"Creating Mechanic Blueprint: {menuName} --- Interactable Object: {this.objectName}");
        var blueprintObject = (GameObject)PrefabUtility.InstantiatePrefab(blueprintTemplate);

        if (menuName.Contains("Drag"))
        {
            HandleDragMechanicBlueprint(blueprintObject);
        }
        else if (menuName.Contains("Tap"))
        {
            HandleTapMechanicBlueprint(blueprintObject);
        }

        HandleAnimInMechanic(blueprintObject);
    }

    private void HandleAnimInMechanic(GameObject blueprintObject)
    {
        var skeletonAnimation = FindAnyObjectByType<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.Log($"There is no skeleton animation in this scene!");
            return;
        }

        WaitSpineAnim[] waitSpineAnims = blueprintObject.GetComponentsInChildren<WaitSpineAnim>();
        for (var i = 0; i < waitSpineAnims.Length; i++)
        {
            WaitSpineAnim waitAnim = waitSpineAnims[i];
            waitAnim.SetField("skeletonAnimation", skeletonAnimation, AccessModifier.Private);
            waitAnim.name = $"{waitAnim.name}_{this.objectName}_{i + 1}";
        }

        PlaySpineAnim[] playSpineAnims = blueprintObject.GetComponentsInChildren<PlaySpineAnim>();
        for (var i = 0; i < playSpineAnims.Length; i++)
        {
            PlaySpineAnim playAnim = playSpineAnims[i];
            playAnim.SetField("skeletonAnimation", skeletonAnimation, AccessModifier.Private);
            playAnim.name = $"{playAnim.name}_{this.objectName}_{i + 1}";
        }
    }

    private void HandleTapMechanicBlueprint(GameObject blueprintObject)
    {
        BoxArea boxArea = CreateBoxArea();
        boxArea.name = $"TapArea_{this.objectName}";

        var checkTapArea = blueprintObject.GetComponentInChildren<TapArea>();
        checkTapArea.SetField("target", boxArea, AccessModifier.Private);

        var gameObjects = new GameObject[] { boxArea.gameObject };
        HandleSetActiveCommandInMechanic(blueprintObject, gameObjects);
        
        AddAutoRenameComponent(blueprintObject, boxArea.gameObject, $"{blueprintObject.name}", "TapArea");
        AddAutoRenameComponent(checkTapArea.gameObject, boxArea.gameObject, $"{checkTapArea.name}", "TapArea");
    }

    private void HandleDragMechanicBlueprint(GameObject blueprintObject)
    {
        var draggableObjectTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{DraggableObjectBlueprintAddress}");
        var draggableObject = (GameObject)PrefabUtility.InstantiatePrefab(draggableObjectTemplate);
        draggableObject.name = $"Draggable_{this.objectName}";
        
        BoxArea boxArea = CreateBoxArea();
        boxArea.name = $"BoxDestination";

        var insideArea2D = blueprintObject.GetComponentInChildren<InsideArea2D>();
        insideArea2D.SetField("checkTransform", draggableObject.transform, AccessModifier.Private);
        insideArea2D.SetField("targetArea", boxArea, AccessModifier.Private);

        var gameObjects = new GameObject[] { draggableObject, boxArea.gameObject };
        HandleSetActiveCommandInMechanic(blueprintObject, gameObjects);

        AddAutoRenameComponent(blueprintObject, draggableObject, $"{blueprintObject.name}", "Draggable");
        AddAutoRenameComponent(boxArea.gameObject, draggableObject, $"{boxArea.name}", "Draggable");
        AddAutoRenameComponent(insideArea2D.gameObject, draggableObject, $"{insideArea2D.name}", "Draggable");
    }

    private void AddAutoRenameComponent(GameObject gameObject, GameObject targetObject, string prefix, string removeString)
    {
        var boxAutoRename = gameObject.gameObject.AddComponent<AutoRenameFollow>();
        boxAutoRename.SetField("target", targetObject, AccessModifier.Private);
        boxAutoRename.SetField("prefix", prefix, AccessModifier.Private);
        boxAutoRename.SetField("removeString", removeString, AccessModifier.Private);
    }

    private void HandleSetActiveCommandInMechanic(GameObject blueprintObject, GameObject[] gameObjects)
    {
        SetActiveMultipleGameObjectsAction[] setActiveCommand = blueprintObject.GetComponentsInChildren<SetActiveMultipleGameObjectsAction>();
        foreach (SetActiveMultipleGameObjectsAction setActive in setActiveCommand)
        {
            setActive.SetField("gameObjects", gameObjects, AccessModifier.Private);
            setActive.name = $"{setActive.name}_{this.objectName}";
        }
    }

    private BoxArea CreateBoxArea()
    {
        var boxAreaObject = new GameObject();
        var boxArea2D = boxAreaObject.AddComponent<BoxArea>();
        boxArea2D.transform.localPosition = Vector3.zero;
        boxArea2D.SetField("boxCollider", boxArea2D.GetComponent<BoxCollider2D>(), AccessModifier.Private);

        return boxArea2D;
    }

    private string GetBlueprintAddress(string menuName)
    {
        if (menuName.Contains("Drag"))
        {
            return DragBlueprintAddress;
        }
        else if (menuName.Contains("Timing"))
        {
            return "";
        }
        else if (menuName.Contains("Tap"))
        {
            return TapBlueprintAddress;
        }

        return "";
    }
}