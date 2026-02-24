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

        if (GUILayout.Button("Generate Level Doc", GUILayout.Height(30)))
        {
            GenerateLevelDoc();
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

    private void GenerateLevelDoc()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("STATIC_OBJECT");

        foreach (Transform child in this.levelEditor.StaticObjectParent)
        {
            if (child == this.levelEditor.StaticObjectParent) continue;

            sb.AppendLine($"{child.name}");
        }

        sb.AppendLine();
        sb.AppendLine("INTERACTABLE_OBJECT");

        foreach (Transform child in this.levelEditor.InteractableObjectParent)
        {
            if (child == this.levelEditor.InteractableObjectParent) continue;

            sb.AppendLine($"{child.name}");
        }

        sb.AppendLine();
        sb.AppendLine("ANIMATION");

        Transform[] allChildren = this.levelEditor.SkeletonAnimation.transform.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            sb.AppendLine($"{child.name}");
        }
        
        sb.AppendLine();
        sb.AppendLine("ROOT_SEQUENCE_LOGIC");
        allChildren = this.levelEditor.RootSequenceParent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child == this.levelEditor.RootSequenceParent) continue;

            sb.AppendLine($"{child.name}");
        }

        string filePath = System.IO.Path.Combine(Application.persistentDataPath, $"{this.levelEditor.gameObject.name}.csv");
        System.IO.File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
        Debug.Log($"Exported to: {filePath}");
    }

    private string GetRelativePath(Transform child, Transform root)
    {
        string path = child.name;
        Transform current = child.parent;
        while (current != null && current != root)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
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