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
    [SerializeField, SpineAnimation] private string animationName;
    private float previousTime = 0.0f;
    private float maxRange = 1.0f;
    private string eventKeyName;

    [ShowInInspector, PropertyRange(0.0, 1.0f, MaxMember = "@maxRange")]
    private float currentTime = 0.0f;

    private string previousAnimationName;
    private TrackEntry trackEntry;

    public SkeletonAnimation SkeletonAnimation
    {
        get => this.skeletonAnimation;
        set => this.skeletonAnimation = value;
    }

    public float CurrentTime
    {
        get => this.currentTime;
        set
        {
            if (Mathf.Abs(this.currentTime - value) > 0.001f)
            {
                this.currentTime = value;
                UpdateTrackingTime();
            }
        }
    }

    public float MaxRange => this.maxRange;

    public string EventKeyName
    {
        get => this.eventKeyName;
        set => this.eventKeyName = value;
    }

    public string AnimationName => this.animationName;

    public TrackEntry TrackEntry => this.trackEntry;

    private void OnValidate()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (this.SkeletonAnimation == null)
        {
            this.SkeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        if (this.SkeletonAnimation == null) return;

        if (this.animationName != this.previousAnimationName)
        {
            this.trackEntry = this.SkeletonAnimation.AnimationState.SetAnimation(0, this.animationName, false);
            this.maxRange = this.trackEntry.Animation.Duration * 0.5f;
            this.CurrentTime = 0.0f;
            this.previousTime = -1.0f;
            this.previousAnimationName = this.animationName;
        }
        else if (this.trackEntry == null && !string.IsNullOrEmpty(this.animationName))
        {
            this.trackEntry = this.SkeletonAnimation.AnimationState.SetAnimation(0, this.animationName, false);
        }
    }

    private void UpdateTrackingTime()
    {
        if (this.trackEntry != null && this.CurrentTime != this.previousTime)
        {
            if (this.SkeletonAnimation.state == null) return;
            this.trackEntry.TrackTime = this.CurrentTime;
            this.SkeletonAnimation.Update(this.CurrentTime);
            // this.SkeletonAnimation.state.Update(this.CurrentTime);
            this.SkeletonAnimation.LateUpdate();
            // this.SkeletonAnimation.skeleton.Update(this.CurrentTime);
            this.previousTime = this.CurrentTime;
        }
    }

    public void AddEventKeyAtCurrentTime()
    {
        if (string.IsNullOrEmpty(this.EventKeyName))
        {
            Debug.LogError("EventKeyName has to be set!");
            return;
        }

        if (this.trackEntry == null || string.IsNullOrEmpty(this.animationName))
        {
            Debug.LogError("No animation is currently playing!");
            return;
        }

        EventData eventData = this.SkeletonAnimation.Skeleton.Data.FindEvent(this.EventKeyName);
        if (eventData == null)
        {
            eventData = new EventData(this.EventKeyName);
            this.SkeletonAnimation.Skeleton.Data.Events.Add(eventData);
            Debug.Log($"Created new EventData: {this.EventKeyName}");
        }

        Animation animation = this.trackEntry.Animation;
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

        float currentTrackTime = this.trackEntry.TrackTime;
        var newEvent = new Event(currentTrackTime, eventData);

        if (eventTimeline == null)
        {
            var newTimeline = new EventTimeline(1);
            newTimeline.SetFrame(0, newEvent);
            timelines.Add(newTimeline);
            Debug.Log($"Created new EventTimeline and added event '{this.EventKeyName}' at time {currentTrackTime:F3}s");
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
            Debug.Log($"Added event '{this.EventKeyName}' at time {currentTrackTime:F3}s (Total: {newEvents.Length} events)");
        }
    }
}