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
    private LevelGenerator levelGenerator;
    private HintGenerator hintGenerator;
    private GameObject psdImporter;
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

        EditorGUILayout.Space(20);

        this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

        this.staticObjectEditorSection = new StaticObjectEditorSection(this.levelEditor);
        DrawSection("Static Objects", ref this.showStaticObjects, this.staticObjectEditorSection);

        EditorGUILayout.Space(20);

        this.interactableObjectEditorSection = new InteractableObjectEditorSection(this.levelEditor);
        DrawSection("Interactable Objects", ref this.showInteractableObjects, this.interactableObjectEditorSection);

        EditorGUILayout.Space(20);

        this.boxEditorSection = new InteractingBoxEditorSection(this.levelEditor);
        DrawSection("Interacting Boxes", ref this.showInteractingBoxes, this.boxEditorSection);

        EditorGUILayout.Space(20);

        this.skeletonAnimationEditorSection = new SkeletonAnimationEditorSection(this.levelEditor);
        DrawSection("Animation", ref this.showAnimation, this.skeletonAnimationEditorSection);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space(10);

        DrawBottomButtons();
    }

    private void DrawSection(string sectionName, ref bool sectionToggle, EditorSection editorSection)
    {
        EditorGUILayout.BeginVertical("box");
        sectionToggle = EditorGUILayout.Foldout(sectionToggle, sectionName, true, EditorStyles.foldoutHeader);
        if (sectionToggle)
        {
            editorSection.DrawSection();
        }

        EditorGUILayout.EndVertical();
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

        if (GUILayout.Button("Generate Level Hint", GUILayout.Height(30)))
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

        EditorGUILayout.EndHorizontal();
    }
}