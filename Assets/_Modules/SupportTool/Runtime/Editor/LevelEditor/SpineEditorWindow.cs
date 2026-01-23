using System.Collections.Generic;
using System.IO;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

public class SpineEditorWindow : EditorWindow
{
    private SpineAnimationEditorPlayer spineAnimationEditor;
    private Vector2 scrollPosition;
    private float currentTime;

    public void Initialize(SpineAnimationEditorPlayer editor)
    {
        this.spineAnimationEditor = editor;
    }

    public void ShowWindow()
    {
        var window = GetWindow<SpineEditorWindow>(true, "Spine Editor", true);
        window.minSize = new Vector2(300, 300);
        window.maxSize = new Vector2(1000, 1200);
        window.ShowPopup();

        this.currentTime = 0;
        this.spineAnimationEditor.EventKeyName = "";
    }

    private void OnGUI()
    {
        if (this.spineAnimationEditor == null)
        {
            EditorGUILayout.HelpBox("No Spine Editor assigned!", MessageType.Warning);
            return;
        }

        this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

        EditorGUILayout.BeginHorizontal();
        DrawSkeletonAnimationSection();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        DrawEventKeyInputField();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        DrawActionFieldSection();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        DrawTrackingTimeSection();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Event Key", GUILayout.Height(30)))
        {
            this.spineAnimationEditor.AddEventKeyAtCurrentTime();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space(10);

        DrawBottomButtons();
    }

    private void DrawActionFieldSection()
    {
        var serializedObject = new SerializedObject(this.spineAnimationEditor);
        SerializedProperty animProp = serializedObject.FindProperty("animationName");
        EditorGUILayout.LabelField("Action:", GUILayout.Width(50));
        EditorGUILayout.PropertyField(animProp, GUIContent.none);
    }

    private void DrawTrackingTimeSection()
    {
        EditorGUILayout.LabelField("Tracking Time ", GUILayout.Width(100));
        float currentTime = this.spineAnimationEditor.CurrentTime;
        EditorGUI.BeginChangeCheck();
        float newTime = EditorGUILayout.Slider(currentTime, 0, this.spineAnimationEditor.MaxRange);
        if (EditorGUI.EndChangeCheck())
        {
            this.spineAnimationEditor.CurrentTime = newTime;
        }
    }

    private void DrawSkeletonAnimationSection()
    {
        EditorGUILayout.LabelField("Skeleton Animation:");
        SkeletonAnimation skeletonAnimation = this.spineAnimationEditor.SkeletonAnimation;
        var newAnimation = (SkeletonAnimation)EditorGUILayout.ObjectField(
            skeletonAnimation,
            typeof(SkeletonAnimation),
            true,
            GUILayout.Height(18)
        );
        if (newAnimation != skeletonAnimation)
        {
            Undo.RecordObject(this.spineAnimationEditor, "Change Animation");
            this.spineAnimationEditor.SkeletonAnimation = newAnimation;
            EditorUtility.SetDirty(this.spineAnimationEditor);
        }
    }

    private void DrawEventKeyInputField()
    {
        EditorGUILayout.LabelField("Event Key Name:", GUILayout.Width(100));
        string newName = EditorGUILayout.TextField(this.spineAnimationEditor.EventKeyName);
        if (!newName.Equals(this.spineAnimationEditor.EventKeyName))
        {
            this.spineAnimationEditor.EventKeyName = newName;
        }
    }

    private void DrawBottomButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Refresh Data", GUILayout.Height(30)))
        {
            GameObject skeletonAnimationGameObject = this.spineAnimationEditor.SkeletonAnimation.gameObject;
            Selection.activeGameObject = skeletonAnimationGameObject;
            EditorGUIUtility.PingObject(skeletonAnimationGameObject);
            Repaint();
        }

        if (GUILayout.Button("Export To Spine Json", GUILayout.Height(30)))
        {
            if (this.spineAnimationEditor == null ||
                this.spineAnimationEditor.SkeletonAnimation == null ||
                this.spineAnimationEditor.SkeletonAnimation.skeletonDataAsset == null)
            {
                return;
            }

            ExportToSpineJson();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ExportToSpineJson()
    {
        var skeletonDataAsset = this.spineAnimationEditor.SkeletonAnimation.skeletonDataAsset;

        if (skeletonDataAsset == null)
        {
            Debug.LogError("SkeletonDataAsset is null!");
            return;
        }

        var textAsset = skeletonDataAsset.skeletonJSON;
        if (textAsset == null)
        {
            Debug.LogError("Cannot find Spine JSON file!");
            return;
        }

        string originalJsonPath = AssetDatabase.GetAssetPath(textAsset);
        string jsonContent = File.ReadAllText(originalJsonPath);

        string modifiedJson = UpdateSpineJsonWithEvents(jsonContent);

        string directory = Path.GetDirectoryName(originalJsonPath);
        string fileName = Path.GetFileNameWithoutExtension(originalJsonPath) + "_Sound.json";
        string newPath = Path.Combine(directory, fileName);

        File.WriteAllText(newPath, modifiedJson);
        AssetDatabase.Refresh();

        Debug.Log($"✓ Exported Spine JSON with events to: {newPath}");
        Debug.LogWarning("Next steps:\n1. Reimport this JSON in Spine Editor\n2. Or update your SkeletonDataAsset to point to this new file");
    }

    private string UpdateSpineJsonWithEvents(string jsonContent)
    {
        var skeletonData = this.spineAnimationEditor.SkeletonAnimation.Skeleton.Data;

        // Collect all unique event names from all animations
        HashSet<string> allEventNames = new HashSet<string>();
        foreach (var animation in skeletonData.Animations)
        {
            foreach (var timeline in animation.Timelines)
            {
                if (timeline is EventTimeline et)
                {
                    foreach (var evt in et.Events)
                    {
                        allEventNames.Add(evt.Data.Name);
                    }
                }
            }
        }

        // Build events section if there are any events
        string eventsSection = "";
        if (allEventNames.Count > 0)
        {
            var eventsBuilder = new System.Text.StringBuilder();
            eventsBuilder.Append("\"events\": {");

            bool firstEvent = true;
            foreach (var eventName in allEventNames)
            {
                if (!firstEvent) eventsBuilder.Append(",");
                eventsBuilder.Append($"\"{eventName}\":{{}}");
                firstEvent = false;
            }

            eventsBuilder.Append("}");
            eventsSection = eventsBuilder.ToString();
        }

        // Find animations section
        int animationsStart = jsonContent.IndexOf("\"animations\"");
        if (animationsStart == -1)
        {
            Debug.LogError("Cannot find 'animations' section in JSON!");
            return jsonContent;
        }

        // Build animations section with events
        var animBuilder = new System.Text.StringBuilder();
        animBuilder.Append("\"animations\": {");

        bool firstAnim = true;
        foreach (var animation in skeletonData.Animations)
        {
            if (!firstAnim) animBuilder.Append(",");
            animBuilder.Append($"\n  \"{animation.Name}\": {{");

            // Find EventTimeline
            EventTimeline eventTimeline = null;
            foreach (var timeline in animation.Timelines)
            {
                if (timeline is EventTimeline et)
                {
                    eventTimeline = et;
                    break;
                }
            }

            // Copy old animation data (bones, slots, etc.)
            string animKey = $"\"{animation.Name}\":";
            int animStart = jsonContent.IndexOf(animKey, animationsStart);

            bool hasOldContent = false;
            string oldAnimContent = "";

            if (animStart != -1)
            {
                int braceStart = jsonContent.IndexOf("{", animStart + animKey.Length);
                if (braceStart != -1)
                {
                    int braceEnd = FindMatchingBrace(jsonContent, braceStart);
                    oldAnimContent = jsonContent.Substring(braceStart + 1, braceEnd - braceStart - 1).Trim();

                    // Remove old events section if exists
                    int oldEventsIndex = oldAnimContent.IndexOf("\"events\"");
                    if (oldEventsIndex != -1)
                    {
                        int eventsStart = oldEventsIndex;
                        int eventsEnd = FindEventsSectionEnd(oldAnimContent, oldEventsIndex);
                        oldAnimContent = oldAnimContent.Remove(eventsStart, eventsEnd - eventsStart);
                        oldAnimContent = oldAnimContent.Trim().TrimEnd(',');
                    }

                    if (!string.IsNullOrWhiteSpace(oldAnimContent))
                    {
                        hasOldContent = true;
                    }
                }
            }

            // Add existing content
            if (hasOldContent)
            {
                animBuilder.Append("\n").Append(oldAnimContent);
                if (eventTimeline != null && eventTimeline.Events.Length > 0)
                    animBuilder.Append(",");
            }

            // Add events section for this animation
            if (eventTimeline != null && eventTimeline.Events.Length > 0)
            {
                animBuilder.Append("\n    \"events\": [");
                var events = eventTimeline.Events;
                for (int i = 0; i < events.Length; i++)
                {
                    var evt = events[i];
                    if (i > 0) animBuilder.Append(",");
                    animBuilder.Append($"\n      {{\"time\": {evt.Time:F4}, \"name\": \"{evt.Data.Name}\"");

                    if (evt.Int != 0) animBuilder.Append($", \"int\": {evt.Int}");
                    if (evt.Float != 0) animBuilder.Append($", \"float\": {evt.Float:F4}");
                    if (!string.IsNullOrEmpty(evt.String)) animBuilder.Append($", \"string\": \"{evt.String}\"");

                    animBuilder.Append("}");
                }

                animBuilder.Append("\n    ]");
            }

            animBuilder.Append("\n  }");
            firstAnim = false;
        }

        animBuilder.Append("\n}");

        // Find where to insert events section (before animations)
        string beforeAnimations = jsonContent.Substring(0, animationsStart).TrimEnd(',', ' ', '\n', '\r', '\t');

        int animationsEnd = FindMatchingBrace(jsonContent, animationsStart + "\"animations\":".Length);
        string afterAnimations = jsonContent.Substring(animationsEnd + 1);

        // Build final JSON
        System.Text.StringBuilder finalJson = new System.Text.StringBuilder();
        finalJson.Append(beforeAnimations);

        // Add events section if we have events
        if (!string.IsNullOrEmpty(eventsSection))
        {
            finalJson.Append(",\n");
            finalJson.Append(eventsSection);
        }

        finalJson.Append(",\n");
        finalJson.Append(animBuilder.ToString());
        finalJson.Append(afterAnimations);

        return finalJson.ToString();
    }

    private int FindMatchingBrace(string json, int startIndex)
    {
        int depth = 1;
        bool inString = false;
        char stringChar = '\0';

        for (int i = startIndex + 1; i < json.Length; i++)
        {
            char c = json[i];

            // Handle string boundaries
            if ((c == '"' || c == '\'') && (i == 0 || json[i - 1] != '\\'))
            {
                if (!inString)
                {
                    inString = true;
                    stringChar = c;
                }
                else if (c == stringChar)
                {
                    inString = false;
                }

                continue;
            }

            if (!inString)
            {
                if (c == '{') depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
        }

        return json.Length - 1;
    }

    private int FindEventsSectionEnd(string content, int eventsIndex)
    {
        int bracketStart = content.IndexOf('[', eventsIndex);
        if (bracketStart == -1) return eventsIndex;

        int depth = 1;
        bool inString = false;
        char stringChar = '\0';

        for (int i = bracketStart + 1; i < content.Length; i++)
        {
            char c = content[i];

            if ((c == '"' || c == '\'') && (i == 0 || content[i - 1] != '\\'))
            {
                if (!inString)
                {
                    inString = true;
                    stringChar = c;
                }
                else if (c == stringChar)
                {
                    inString = false;
                }

                continue;
            }

            if (!inString)
            {
                if (c == '[') depth++;
                else if (c == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        // Find next comma or closing brace
                        for (int j = i + 1; j < content.Length; j++)
                        {
                            if (content[j] == ',' || content[j] == '}')
                                return content[j] == ',' ? j + 1 : j;
                            if (!char.IsWhiteSpace(content[j]))
                                return i + 1;
                        }

                        return i + 1;
                    }
                }
            }
        }

        return content.Length;
    }
}