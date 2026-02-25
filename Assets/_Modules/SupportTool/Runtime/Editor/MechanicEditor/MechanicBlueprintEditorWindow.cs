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
    private AnimationMechanicGenerator animationMechanicGenerator;
    private AudioMechanicGenerator audioMechanicGenerator;
    private SetActiveMechanicGenerator setActiveMechanicGenerator;
    private int selectedTab = 0;
    private Sprite sprite;

    private readonly string[] tabNames = new string[]
    {
        "OBJECTS",
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

        switch (this.tabNames[this.selectedTab])
        {
            case "DRAG":
                DrawSpriteSection();
                EditorGUILayout.Space(10);
                DrawDraggingMechanicSection();
                break;

            case "TAP":
                DrawSpriteSection();
                EditorGUILayout.Space(10);
                DrawTappingMechanicSection();
                break;

            case "MIX":
                DrawSpriteSection();
                EditorGUILayout.Space(10);
                DrawMixingMechanicSection();
                break;
            case "ANIMATION":
                DrawAnimationSection();
                break;
            case "AUDIO":
                DrawAudioSection();
                break;
            case "SET_ACTIVE":
                DrawSettingActiveSection();
                break;
            case "OBJECTS":
                DrawSpriteSection();
                EditorGUILayout.Space(10);
                DrawButton("Draggable_Object", () => OnMenuItemClicked("Draggable_Object"));
                DrawButton("Static_Object", () => OnMenuItemClicked("Static_Object"));
                break;
        }
    }

    private void DrawSettingActiveSection()
    {
        DrawButton("SetActive_On", () => OnMenuItemClicked("SetActive_On"));
        DrawButton("SetActive_Off", () => OnMenuItemClicked("SetActive_Off"));
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("SetActive_On: Active các Gameobject được chỉ định");
        EditorGUILayout.LabelField("SetActive_Off: Disable các GameObject được chỉ định");
    }

    private void DrawAudioSection()
    {
        DrawButton("PlayAudio", () => OnMenuItemClicked("PlayAudio"));
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("PlayAudio: Chạy sound 1 lần");
    }

    private void DrawAnimationSection()
    {
        DrawButton("PlayAndWaitAnim", () => OnMenuItemClicked("PlayAndWaitAnim"));
        DrawButton("LoopAnim", () => OnMenuItemClicked("LoopAnim"));
        DrawButton("PlayAndNotWaitAnim", () => OnMenuItemClicked("PlayAndNotWaitAnim"));
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("PlayAndWaitAnim: Chạy 1 action của anim và đợi đến khi action này chạy hết");
        EditorGUILayout.LabelField("LoopAnim: Chạy 1 action của anim dưới dạng loop và không phải đợi action này chạy hết");
        EditorGUILayout.LabelField("PlayAndNotWaitAnim: Chạy 1 action của anim và không phải đợi action này chạy hết");
    }

    private void DrawMixingMechanicSection()
    {
        DrawButton("Mix_Tap_Drag_True", () => OnMenuItemClicked("Mix_Tap_Drag_True"));
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Mix_Tap_Drag_True/False: Tạo ra Mechanic có 2 step, Tap 1 khu vực, sau đó kéo 1 vật dẫn đến kết quả True/False");
    }

    private void DrawTappingMechanicSection()
    {
        DrawButton("Tap_True", () => OnMenuItemClicked("Tap_True"));
        DrawButton("Tap_False", () => OnMenuItemClicked("Tap_False"));
        DrawButton("Tap_NoResult", () => OnMenuItemClicked("Tap_NoResult"));
        DrawButton("Tap_True_Moving", () => OnMenuItemClicked("Tap_True_Moving"));
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Tap_True/False: Tạo ra Mechanic Tap cho 1 khu vực dẫn đến kết quả True/False");
        EditorGUILayout.LabelField("Tap_True/False_Moving: Tạo ra Mechanic Tap cho 1 vật di chuyển dẫn đến kết quả True/False");
    }

    private void DrawDraggingMechanicSection()
    {
        DrawButton("Drag_True", () => OnMenuItemClicked("Drag_True"));
        DrawButton("Drag_False", () => OnMenuItemClicked("Drag_False"));
        DrawButton("Drag_NoResult", () => OnMenuItemClicked("Drag_NoResult"));
        DrawButton("Drag_2_Result", () => OnMenuItemClicked("Drag_2Result"));
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Help:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Drag_True/False: Tạo ra Mechanic Drag cho 1 vật dẫn đến kết quả True/False");
        EditorGUILayout.LabelField("Drag_2_Result: Tạo ra Mechanic Drag cho 1 vật dẫn đến 1 trong 2 kết quả True/False trong Level");
    }

    private void DrawSpriteSection()
    {
        EditorGUILayout.LabelField("Sprite:", GUILayout.Width(60));

        var newSprite = (Sprite)EditorGUILayout.ObjectField(
            this.sprite,
            typeof(Sprite),
            false
        );
        if (newSprite != this.sprite)
        {
            this.sprite = newSprite;
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
        GameObject staticObjectsParent = GameObject.Find("StaticObjects");
        GameObject boxInteractionParent = GameObject.Find("BoxInteraction");

        if (interactableObjectsParent == null || boxInteractionParent == null)
        {
            Debug.LogError("You have to create level blueprint!");
            return;
        }

        switch (this.tabNames[this.selectedTab])
        {
            case "DRAG":
                this.dragMechanicGenerator = new DragMechanicGenerator();
                this.dragMechanicGenerator.CreateMechanic(menuName, this.objectName, skeletonAnimation, interactableObjectsParent, boxInteractionParent, this.sprite);
                break;
            case "TAP":
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
            case "MIX":
                this.mixingMechanicGenerator = new MixingMechanicGenerator();
                this.mixingMechanicGenerator.CreateMechanic(menuName, this.objectName, skeletonAnimation, interactableObjectsParent, boxInteractionParent, this.sprite);
                break;
            case "ANIMATION":
                this.animationMechanicGenerator = new AnimationMechanicGenerator();
                this.audioMechanicGenerator = new AudioMechanicGenerator();
                this.animationMechanicGenerator.CreateMechanic(menuName, this.objectName);
                break;
            case "AUDIO":
                this.audioMechanicGenerator = new AudioMechanicGenerator();
                this.audioMechanicGenerator.CreateMechanic(menuName, this.objectName);
                break;
            case "SET_ACTIVE":
                this.setActiveMechanicGenerator = new SetActiveMechanicGenerator();
                this.setActiveMechanicGenerator.CreateMechanic(menuName, this.objectName);
                break;
            case "OBJECTS":
                if (menuName.Contains("Static"))
                {
                    var newStaticObj = new GameObject(this.objectName);
                    newStaticObj.transform.SetParent(staticObjectsParent.transform);
                    if (this.sprite != null)
                    {
                        var objectRenderer = newStaticObj.AddComponent<SpriteRenderer>();
                        objectRenderer.sprite = this.sprite;
                    }
                }
                else if (menuName.Contains("Draggable"))
                {
                    this.dragMechanicGenerator = new DragMechanicGenerator();
                    this.dragMechanicGenerator.CreateDraggableObject(this.objectName, interactableObjectsParent, this.sprite);
                }

                break;
        }
    }
}