using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using Animation = Spine.Animation;

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
        window.maxSize = new Vector2(2000, 1200);
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
        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        DrawTimelineSection();
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

    private void DrawTimelineSection()
    {
        GUILayout.BeginVertical("Box");

        var mainAnim = this.spineAnimationEditor.SkeletonAnimation;
        Animation currentAnimation = null;
        EventTimeline eventTimeline = null;

        if (mainAnim != null && mainAnim.skeletonDataAsset != null)
        {
            string currentAnimName = this.spineAnimationEditor.AnimationName;

            if (!string.IsNullOrEmpty(currentAnimName))
            {
                var skeletonData = mainAnim.skeletonDataAsset.GetSkeletonData(true);

                if (skeletonData != null)
                {
                    currentAnimation = skeletonData.FindAnimation(currentAnimName);

                    if (currentAnimation != null)
                    {
                        foreach (var timeline in currentAnimation.Timelines)
                        {
                            if (timeline is EventTimeline et)
                            {
                                eventTimeline = et;
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (currentAnimation == null)
        {
            GUILayout.EndVertical();
            return;
        }

        float duration = currentAnimation.Duration;
        float timelineWidth = position.width - 60;
        float timelineHeight = 60;

        Rect timelineRect = GUILayoutUtility.GetRect(timelineWidth, timelineHeight);
        EditorGUI.DrawRect(timelineRect, new Color(0.2f, 0.2f, 0.2f, 1f));

        DrawTimeMarkers(timelineRect, duration);

        if (eventTimeline != null && eventTimeline.Events.Length > 0)
        {
            DrawEventMarkers(timelineRect, eventTimeline, duration);
        }

        if (this.spineAnimationEditor.TrackEntry != null)
        {
            DrawPlayhead(timelineRect, this.spineAnimationEditor.TrackEntry.TrackTime, duration);
        }

        GUILayout.EndVertical();
    }

    private void DrawTimeMarkers(Rect timelineRect, float duration)
    {
        float interval = 0.5f;
        int numMarkers = Mathf.CeilToInt(duration / interval);

        for (int i = 0; i <= numMarkers; i++)
        {
            float time = i * interval;
            if (time > duration) break;

            float x = timelineRect.x + (time / duration) * timelineRect.width;

            Rect lineRect = new Rect(x, timelineRect.y, 1, timelineRect.height);
            EditorGUI.DrawRect(lineRect, new Color(0.4f, 0.4f, 0.4f, 0.5f));

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 9;
            labelStyle.normal.textColor = Color.gray;
            labelStyle.alignment = TextAnchor.UpperCenter;

            Rect labelRect = new Rect(x - 20, timelineRect.y + 2, 40, 15);
            GUI.Label(labelRect, $"{time:F1}s", labelStyle);
        }
    }

    private void DrawEventMarkers(Rect timelineRect, EventTimeline eventTimeline, float duration)
    {
        for (int i = 0; i < eventTimeline.Events.Length; i++)
        {
            var evt = eventTimeline.Events[i];

            if (evt.Time > duration)
            {
                continue;
            }

            float normalizedTime = evt.Time / duration;
            float x = timelineRect.x + normalizedTime * timelineRect.width;

            float markerWidth = 8;
            float markerHeight = 20;
            Rect markerRect = new Rect(
                x - markerWidth / 2,
                timelineRect.y + timelineRect.height / 2 - markerHeight / 2,
                markerWidth,
                markerHeight
            );

            EditorGUI.DrawRect(markerRect, new Color(0.6f, 0.3f, 0.8f, 1f));
            Handles.BeginGUI();
            Handles.color = new Color(0.8f, 0.5f, 1f, 1f);
            Handles.DrawSolidRectangleWithOutline(markerRect, Color.clear, Handles.color);
            Handles.EndGUI();

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 9;
            labelStyle.normal.textColor = Color.white;
            labelStyle.alignment = TextAnchor.LowerCenter;
            labelStyle.fontStyle = FontStyle.Bold;

            Vector2 labelSize = labelStyle.CalcSize(new GUIContent(evt.Data.Name));
            Rect labelRect = new Rect(
                x - labelSize.x / 2,
                timelineRect.y - 15,
                labelSize.x,
                15
            );

            EditorGUI.DrawRect(new Rect(labelRect.x - 2, labelRect.y, labelRect.width + 4, labelRect.height),
                new Color(0.1f, 0.1f, 0.1f, 0.8f));
            GUI.Label(labelRect, evt.Data.Name, labelStyle);

            if (markerRect.Contains(UnityEngine.Event.current.mousePosition))
            {
                GUIStyle tooltipStyle = new GUIStyle(GUI.skin.box);
                tooltipStyle.normal.textColor = Color.white;
                tooltipStyle.fontSize = 10;
                tooltipStyle.alignment = TextAnchor.MiddleCenter;

                string eventInfo = $"{evt.Data.Name}\n{evt.Time:F3}s";
                Vector2 tooltipSize = tooltipStyle.CalcSize(new GUIContent(eventInfo));
                Rect tooltipRect = new Rect(
                    x - tooltipSize.x / 2,
                    timelineRect.y + timelineRect.height + 5,
                    tooltipSize.x + 10,
                    tooltipSize.y + 5
                );

                EditorGUI.DrawRect(tooltipRect, new Color(0.1f, 0.1f, 0.1f, 0.95f));
                GUI.Label(tooltipRect, eventInfo, tooltipStyle);
            }
        }
    }

    private void DrawPlayhead(Rect timelineRect, float currentTime, float duration)
    {
        float x = timelineRect.x + (currentTime / duration) * timelineRect.width;

        Rect playheadRect = new Rect(x - 1, timelineRect.y, 2, timelineRect.height);
        EditorGUI.DrawRect(playheadRect, Color.red);

        Vector3[] trianglePoints = new Vector3[3];
        trianglePoints[0] = new Vector3(x - 5, timelineRect.y, 0);
        trianglePoints[1] = new Vector3(x + 5, timelineRect.y, 0);
        trianglePoints[2] = new Vector3(x, timelineRect.y + 8, 0);

        Handles.color = Color.red;
        Handles.DrawAAConvexPolygon(trianglePoints);
    }

    private void ExportToSpineJson()
    {
        var skeletonDataAsset = this.spineAnimationEditor.SkeletonAnimation.skeletonDataAsset;
        if (skeletonDataAsset == null || skeletonDataAsset.skeletonJSON == null)
        {
            Debug.LogError("Skeleton Data Asset or Skeleton JSON is null!");
            return;
        }

        TextAsset textAsset = skeletonDataAsset.skeletonJSON;
        if (textAsset == null)
        {
            Debug.LogError("TextAsset from SkeletonDataAsset is null!");
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
    }

    private string UpdateSpineJsonWithEvents(string jsonContent)
    {
        var skeletonData = this.spineAnimationEditor.SkeletonAnimation.Skeleton.Data;

        // Collect all unique event names
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

        // Build properly formatted events section
        StringBuilder eventsSection = new StringBuilder();
        if (allEventNames.Count > 0)
        {
            eventsSection.Append("\"events\": {\n");
            bool first = true;
            foreach (var eventName in allEventNames)
            {
                if (!first) eventsSection.Append(",\n");
                eventsSection.Append($"\t\"{eventName}\": {{}}");
                first = false;
            }
            eventsSection.Append("\n}");
        }

        // Find animations section
        int animationsStart = jsonContent.IndexOf("\"animations\"");
        if (animationsStart == -1)
        {
            Debug.LogError("Cannot find 'animations' section!");
            return jsonContent;
        }

        // Get content before animations
        string beforeAnimations = jsonContent.Substring(0, animationsStart);
        
        // Remove trailing comma/whitespace
        beforeAnimations = beforeAnimations.TrimEnd();
        if (beforeAnimations.EndsWith(","))
        {
            beforeAnimations = beforeAnimations.Substring(0, beforeAnimations.Length - 1);
        }

        // Build animations section
        StringBuilder animationsSection = BuildAnimationsSection(skeletonData, jsonContent, animationsStart);

        // Find content after animations
        int animBraceStart = jsonContent.IndexOf("{", animationsStart + "\"animations\"".Length);
        int animBraceEnd = FindMatchingBrace(jsonContent, animBraceStart);
        string afterAnimations = jsonContent.Substring(animBraceEnd + 1);

        // Assemble final JSON
        StringBuilder finalJson = new StringBuilder();
        finalJson.Append(beforeAnimations);
        
        // Add events section if we have events
        if (eventsSection.Length > 0)
        {
            finalJson.Append(",\n");
            finalJson.Append(eventsSection);
        }
        
        finalJson.Append(",\n");
        finalJson.Append(animationsSection);
        finalJson.Append(afterAnimations);

        return finalJson.ToString();
    }

    private StringBuilder BuildAnimationsSection(SkeletonData skeletonData, string originalJson, int animationsStart)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("\"animations\": {");

        bool firstAnim = true;
        foreach (var animation in skeletonData.Animations)
        {
            if (!firstAnim) builder.Append(",");
            builder.Append($"\n  \"{animation.Name}\": {{");

            // Get EventTimeline for this animation
            EventTimeline eventTimeline = null;
            foreach (var timeline in animation.Timelines)
            {
                if (timeline is EventTimeline et)
                {
                    eventTimeline = et;
                    break;
                }
            }

            // Extract original animation content (bones, slots, etc.)
            string originalAnimContent = ExtractOriginalAnimationContent(originalJson, animation.Name, animationsStart);

            // Add original content
            bool hasContent = !string.IsNullOrWhiteSpace(originalAnimContent);
            if (hasContent)
            {
                builder.Append("\n");
                builder.Append(originalAnimContent);
            }

            // Add events if present
            if (eventTimeline != null && eventTimeline.Events.Length > 0)
            {
                if (hasContent)
                {
                    builder.Append(",");
                }
                builder.Append("\n    \"events\": [");
                
                for (int i = 0; i < eventTimeline.Events.Length; i++)
                {
                    var evt = eventTimeline.Events[i];
                    if (i > 0) builder.Append(",");
                    
                    builder.Append($"\n      {{\"time\": {evt.Time:F4}, \"name\": \"{evt.Data.Name}\"");
                    
                    if (evt.Int != 0) builder.Append($", \"int\": {evt.Int}");
                    if (evt.Float != 0) builder.Append($", \"float\": {evt.Float:F4}");
                    if (!string.IsNullOrEmpty(evt.String)) builder.Append($", \"string\": \"{evt.String}\"");
                    
                    builder.Append("}");
                }
                
                builder.Append("\n    ]");
            }

            builder.Append("\n  }");
            firstAnim = false;
        }

        builder.Append("\n}");
        return builder;
    }

    private string ExtractOriginalAnimationContent(string json, string animName, int animationsStart)
    {
        string animKey = $"\"{animName}\":";
        int animStart = json.IndexOf(animKey, animationsStart);
        
        if (animStart == -1) return "";

        int braceStart = json.IndexOf("{", animStart + animKey.Length);
        if (braceStart == -1) return "";

        int braceEnd = FindMatchingBrace(json, braceStart);
        string content = json.Substring(braceStart + 1, braceEnd - braceStart - 1);

        // Remove any existing events section
        content = RemoveEventsSection(content);

        // Clean up trailing commas and whitespace
        content = content.Trim().TrimEnd(',');

        return content;
    }

    private string RemoveEventsSection(string content)
    {
        int eventsIndex = content.IndexOf("\"events\"");
        if (eventsIndex == -1) return content;

        // Find the start of the events array
        int arrayStart = content.IndexOf("[", eventsIndex);
        if (arrayStart == -1) return content;

        // Find the matching closing bracket
        int arrayEnd = FindMatchingBracket(content, arrayStart);
        if (arrayEnd == -1) return content;

        // Remove the entire events section including any trailing comma
        int removeStart = eventsIndex;
        int removeEnd = arrayEnd + 1;

        // Check for comma after the events section
        while (removeEnd < content.Length && char.IsWhiteSpace(content[removeEnd]))
        {
            removeEnd++;
        }
        if (removeEnd < content.Length && content[removeEnd] == ',')
        {
            removeEnd++;
        }

        // Check for comma before the events section
        int checkStart = removeStart - 1;
        while (checkStart >= 0 && char.IsWhiteSpace(content[checkStart]))
        {
            checkStart--;
        }
        if (checkStart >= 0 && content[checkStart] == ',')
        {
            removeStart = checkStart;
        }

        return content.Remove(removeStart, removeEnd - removeStart);
    }

    private int FindMatchingBrace(string json, int startIndex)
    {
        return FindMatchingChar(json, startIndex, '{', '}');
    }

    private int FindMatchingBracket(string json, int startIndex)
    {
        return FindMatchingChar(json, startIndex, '[', ']');
    }

    private int FindMatchingChar(string text, int startIndex, char openChar, char closeChar)
    {
        int depth = 1;
        bool inString = false;

        for (int i = startIndex + 1; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '"' && (i == 0 || text[i - 1] != '\\'))
            {
                inString = !inString;
            }

            if (!inString)
            {
                if (c == openChar) depth++;
                else if (c == closeChar)
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
        }

        return text.Length - 1;
    }
}