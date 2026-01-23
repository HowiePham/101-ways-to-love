using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using Animation = Spine.Animation;
using Event = Spine.Event;

[ExecuteInEditMode]
public class SpineAnimationEditorPlayer : MonoBehaviour
{
    [ShowInInspector] private SkeletonAnimation skeletonAnimation;
    [ShowInInspector, SpineAnimation] public string animationName;
    private float previousTime = 0.0f;
    private float maxRange = 1.0f;

    [SerializeField] private string eventKeyName;

    [ShowInInspector, PropertyRange(0.0, 1.0f, MaxMember = "@maxRange")]
    private float currentTime = 0.0f;

    private string previousAnimationName;
    private TrackEntry anim;

    private void OnValidate()
    {
        if (this.skeletonAnimation == null)
        {
            this.skeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        if (this.skeletonAnimation == null) return;

        if (this.animationName != this.previousAnimationName)
        {
            this.anim = this.skeletonAnimation.AnimationState.SetAnimation(0, this.animationName, false);
            this.maxRange = this.anim.Animation.Duration * 0.5f;
            // this.maxRange = this.anim.Animation.Duration;
            this.currentTime = 0.0f;
            this.previousTime = -1.0f;
            this.previousAnimationName = this.animationName;
        }
        else if (this.anim == null && !string.IsNullOrEmpty(this.animationName))
        {
            this.anim = this.skeletonAnimation.AnimationState.SetAnimation(0, this.animationName, false);
        }

        if (this.anim != null && this.currentTime != this.previousTime)
        {
            if (this.skeletonAnimation.state == null) return;
            this.anim.TrackTime = this.currentTime;
            this.skeletonAnimation.Update(this.currentTime);
            this.skeletonAnimation.state.Update(this.currentTime);
            this.skeletonAnimation.LateUpdate();
            this.skeletonAnimation.skeleton.Update(this.currentTime);
            this.previousTime = this.currentTime;
        }
    }

    [Button]
    private void AddEventKeyAtCurrentTime()
    {
        if (string.IsNullOrEmpty(this.eventKeyName))
        {
            Debug.LogError("EventKeyName has to be set!");
            return;
        }

        if (this.anim == null || string.IsNullOrEmpty(this.animationName))
        {
            Debug.LogError("No animation is currently playing!");
            return;
        }

        EventData eventData = this.skeletonAnimation.Skeleton.Data.FindEvent(this.eventKeyName);
        if (eventData == null)
        {
            eventData = new EventData(this.eventKeyName);
            this.skeletonAnimation.Skeleton.Data.Events.Add(eventData);
            Debug.Log($"Created new EventData: {this.eventKeyName}");
        }

        Animation animation = this.anim.Animation;
        ExposedList<Timeline> timelines = animation.Timelines;

        EventTimeline eventTimeline = null;
        int timelineIndex = -1;

        for (int i = 0; i < timelines.Count; i++)
        {
            if (timelines.Items[i] is EventTimeline et)
            {
                eventTimeline = et;
                timelineIndex = i;
                break;
            }
        }

        float currentTrackTime = this.anim.TrackTime;
        var newEvent = new Event(currentTrackTime, eventData);

        if (eventTimeline == null)
        {
            var newTimeline = new EventTimeline(1);
            newTimeline.SetFrame(0, newEvent);
            timelines.Add(newTimeline);
            Debug.Log($"Created new EventTimeline and added event '{this.eventKeyName}' at time {currentTrackTime:F3}s");
        }
        else
        {
            int eventCount = eventTimeline.Events.Length;
            var oldEvents = eventTimeline.Events;
            var newEvents = new Event[eventCount + 1];

            int insertIndex = 0;
            bool inserted = false;

            for (int i = 0; i < eventCount; i++)
            {
                if (!inserted && oldEvents[i].Time > currentTrackTime)
                {
                    newEvents[insertIndex++] = newEvent;
                    inserted = true;
                }

                newEvents[insertIndex++] = oldEvents[i];
            }

            if (!inserted)
            {
                newEvents[insertIndex] = newEvent;
            }

            var newTimeline = new EventTimeline(newEvents.Length);
            for (int i = 0; i < newEvents.Length; i++)
            {
                newTimeline.SetFrame(i, newEvents[i]);
            }

            timelines.Items[timelineIndex] = newTimeline;
            Debug.Log($"Added event '{this.eventKeyName}' at time {currentTrackTime:F3}s (Total: {newEvents.Length} events)");
        }
    }

#if UNITY_EDITOR
    [Button]
    private void ExportToSpineJson()
    {
        var skeletonDataAsset = this.skeletonAnimation.skeletonDataAsset;

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
        var skeletonData = this.skeletonAnimation.Skeleton.Data;

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
#endif
}