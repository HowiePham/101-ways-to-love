using Spine.Unity;
using UnityEditor;
using UnityEngine;

public class MechanicBlueprintEditorWindow : EditorWindow
{
    private string objectName = "";
    private DragMechanicGenerator dragMechanicGenerator;
    private TapMechanicGenerator tapMechanicGenerator;
    private TimingMechanicGenerator timingMechanicGenerator;

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
        DrawButton("Tap_True_Moving", () => OnMenuItemClicked("Tap_True_Moving"));
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
            Debug.LogError($"You have to name this interactable object");
            return;
        }

        var skeletonAnimation = FindAnyObjectByType<SkeletonAnimation>();
        GameObject interactableObjectsParent = GameObject.Find("InteractableObjects");
        GameObject boxInteractionParent = GameObject.Find("BoxInteraction");

        if (interactableObjectsParent == null || boxInteractionParent == null)
        {
            Debug.LogError("You have to create level blueprint!");
            return;
        }

        if (menuName.Contains("Moving"))
        {
            this.timingMechanicGenerator = new TimingMechanicGenerator();
            this.timingMechanicGenerator.CreateMechanic(menuName, this.objectName, skeletonAnimation, interactableObjectsParent, boxInteractionParent);
        }
        else if (menuName.Contains("Drag"))
        {
            this.dragMechanicGenerator = new DragMechanicGenerator();
            this.dragMechanicGenerator.CreateMechanic(menuName, this.objectName, skeletonAnimation, interactableObjectsParent, boxInteractionParent);
        }
        else if (menuName.Contains("Tap"))
        {
            this.tapMechanicGenerator = new TapMechanicGenerator();
            this.tapMechanicGenerator.CreateMechanic(menuName, this.objectName, skeletonAnimation, boxInteractionParent);
        }
    }
}