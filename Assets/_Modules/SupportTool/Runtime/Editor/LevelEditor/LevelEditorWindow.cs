using DarkTonic.MasterAudio;
using Games;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private LevelEditor levelEditor;
    private StaticObjectEditorSection staticObjectEditorSection;
    private InteractableObjectEditorSection interactableObjectEditorSection;
    private SkeletonAnimationEditorSection skeletonAnimationEditorSection;
    private InteractingBoxEditorSection boxEditorSection;
    private SoundEditorSection soundEditorSection;
    private LevelGenerator levelGenerator;
    private HintGenerator hintGenerator;
    private GameObject psdImporter;
    private Vector2 scrollPosition;
    private int selectedTab = 0;

    private readonly string[] tabNames = new string[]
    {
        "Sound",
        "Static Objects",
        "Interactable Objects",
        "Interacting Boxes",
        "Animation"
    };

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

        this.levelGenerator = new LevelGenerator();
        this.hintGenerator = new HintGenerator();

        EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // --- PSD Section ---
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("PSD: ", EditorStyles.boldLabel);

        var psdImporter = (GameObject)EditorGUILayout.ObjectField(
            this.psdImporter,
            typeof(GameObject),
            false,
            GUILayout.Height(18)
        );

        if (psdImporter != this.psdImporter)
        {
            this.psdImporter = psdImporter;
        }

        if (this.psdImporter != null)
        {
            if (GUILayout.Button("Generate Static Object", GUILayout.Width(150)))
            {
                this.levelGenerator.GenerateStaticObjects(this.psdImporter, this.levelEditor.gameObject);
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        this.selectedTab = GUILayout.Toolbar(this.selectedTab, this.tabNames, GUILayout.Height(28));

        EditorGUILayout.Space(5);

        this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

        EditorGUILayout.BeginVertical("box");

        switch (this.selectedTab)
        {
            case 0:
                this.soundEditorSection = new SoundEditorSection(this.levelEditor);
                this.soundEditorSection.DrawSection();
                break;

            case 1:
                this.staticObjectEditorSection = new StaticObjectEditorSection(this.levelEditor);
                this.staticObjectEditorSection.DrawSection();
                break;

            case 2:
                this.interactableObjectEditorSection = new InteractableObjectEditorSection(this.levelEditor);
                this.interactableObjectEditorSection.DrawSection();
                break;

            case 3:
                this.boxEditorSection = new InteractingBoxEditorSection(this.levelEditor);
                this.boxEditorSection.DrawSection();
                break;

            case 4:
                this.skeletonAnimationEditorSection = new SkeletonAnimationEditorSection(this.levelEditor);
                this.skeletonAnimationEditorSection.DrawSection();
                break;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);
        DrawBottomButtons();
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
            GenerateLevelSound();
        }

        if (GUILayout.Button("Generate Level Hint", GUILayout.Height(30)))
        {
            GenerateLevelHint();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void GenerateLevelHint()
    {
        TrueAction[] trueActions = FindObjectsByType<TrueAction>(FindObjectsSortMode.None);
        var hintPlayer = this.levelEditor.GetComponent<HintPlayer>();

        for (int i = this.levelEditor.HintParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(this.levelEditor.HintParent.GetChild(i).gameObject);
        }

        this.hintGenerator.Generate(trueActions, this.levelEditor.HintParent);
        hintPlayer.GetHints();
    }

    private void GenerateLevelSound()
    {
        var creatingPlaySoundSpine = this.levelEditor.SpineLevelSound.GetComponent<CreatePlaySoundSpine>();
        for (int i = this.levelEditor.SpineLevelSound.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(this.levelEditor.SpineLevelSound.GetChild(i).gameObject);
        }

        var dynamicSoundGroupCreator = this.levelEditor.GetComponentInChildren<DynamicSoundGroupCreator>();
        if (dynamicSoundGroupCreator != null)
        {
            DestroyImmediate(dynamicSoundGroupCreator.gameObject);
        }

        if (creatingPlaySoundSpine == null)
        {
            Debug.LogError($"Do not have any Sound Spine Generator!");
        }
        else
        {
            creatingPlaySoundSpine.Generate();
        }
    }
}