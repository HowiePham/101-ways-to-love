using Spine.Unity;
using UnityEditor;
using UnityEngine;

public class MechanicBlueprintEditorWindow : EditorWindow
{
    private string objectName = "";
    private DragMechanicGenerator dragMechanicGenerator;
    private TapMechanicGenerator tapMechanicGenerator;
    private TimingMechanicGenerator timingMechanicGenerator;
    private MixingMechanicGenerator mixingMechanicGenerator;
    private int selectedTab = 0;

    private readonly string[] tabNames = new string[]
    {
        "DRAG",
        "TAP",
        "MIX",
        "ANIMATION",
        "AUDIO",
        "SET_ACTIVE",
    };

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

        this.selectedTab = GUILayout.Toolbar(this.selectedTab, this.tabNames, GUILayout.Height(28));
        EditorGUILayout.Space(5);

        switch (this.selectedTab)
        {
            case 0:
                DrawButton("Drag_True", () => OnMenuItemClicked("Drag_True"));
                DrawButton("Drag_False", () => OnMenuItemClicked("Drag_False"));
                DrawButton("Drag_NoResult", () => OnMenuItemClicked("Drag_NoResult"));
                DrawButton("Drag_2_Result", () => OnMenuItemClicked("Drag_2Result"));
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Drag_True/False: Tạo ra Mechanic Drag cho 1 vật dẫn đến kết quả True/False");
                EditorGUILayout.LabelField("Drag_2_Result: Tạo ra Mechanic Drag cho 1 vật dẫn đến 1 trong 2 kết quả True/False trong Level");
                break;

            case 1:
                DrawButton("Tap_True", () => OnMenuItemClicked("Tap_True"));
                DrawButton("Tap_False", () => OnMenuItemClicked("Tap_False"));
                DrawButton("Tap_NoResult", () => OnMenuItemClicked("Tap_NoResult"));
                DrawButton("Tap_True_Moving", () => OnMenuItemClicked("Tap_True_Moving"));
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Tap_True/False: Tạo ra Mechanic Tap cho 1 khu vực dẫn đến kết quả True/False");
                EditorGUILayout.LabelField("Tap_True/False_Moving: Tạo ra Mechanic Tap cho 1 vật di chuyển dẫn đến kết quả True/False");
                break;

            case 2:
                DrawButton("Mix_Tap_Drag_True", () => OnMenuItemClicked("Mix_Tap_Drag_True"));
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Mix_Tap_Drag_True/False: Tạo ra Mechanic có 2 step, Tap 1 khu vực, sau đó kéo 1 vật dẫn đến kết quả True/False");
                break;
            case 3:
                DrawButton("PlayAndWaitAnim", () => OnMenuItemClicked("PlayAndWaitAnim"));
                DrawButton("LoopAnim", () => OnMenuItemClicked("PlayAndWaitAnim"));
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
                break;
            case 4:
                DrawButton("PlayAudio", () => OnMenuItemClicked("PlayAudio"));
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
                break;
            case 5:
                DrawButton("SetActive_On", () => OnMenuItemClicked("SetActive_On"));
                DrawButton("SetActive_Off", () => OnMenuItemClicked("SetActive_Off"));
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
                break;
        }
    }

    private void DrawButton(string label, System.Action onClick)
    {
        bool clicked = GUILayout.Button(label);

        if (clicked)
        {
            onClick?.Invoke();
            // Close();
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

        switch (this.selectedTab)
        {
            case 0:
                this.dragMechanicGenerator = new DragMechanicGenerator();
                this.dragMechanicGenerator.CreateMechanic(menuName, this.objectName, skeletonAnimation, interactableObjectsParent, boxInteractionParent);
                break;

            case 1:
                if (menuName.Contains("Moving"))
                {
                    this.timingMechanicGenerator = new TimingMechanicGenerator();
                    this.timingMechanicGenerator.CreateMechanic(menuName, this.objectName, skeletonAnimation, interactableObjectsParent, boxInteractionParent);
                }
                else
                {
                    this.tapMechanicGenerator = new TapMechanicGenerator();
                    this.tapMechanicGenerator.CreateMechanic(menuName, this.objectName, skeletonAnimation, boxInteractionParent);
                }

                break;

            case 2:
                this.mixingMechanicGenerator = new MixingMechanicGenerator();
                this.mixingMechanicGenerator.CreateMechanic(menuName, this.objectName, skeletonAnimation, interactableObjectsParent, boxInteractionParent);
                break;
            case 3:

                break;
            case 4:

                break;
            case 5:

                break;
        }
    }
}