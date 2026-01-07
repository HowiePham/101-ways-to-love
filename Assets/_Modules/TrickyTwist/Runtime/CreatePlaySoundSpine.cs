using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DarkTonic.MasterAudio;
using Mimi.VisualActions.ControlFlow;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using VisualFlow;
using Directory = System.IO.Directory;

public class CreatePlaySoundSpine : MonoBehaviour
{
    [SerializeField] private Transform levelRoot;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private DynamicSoundGroupCreator dsgPrefab;
    [SerializeField] private int busIndex = 3;

    private const string namePattern = @"(S[f|F][x|X]_)+([^\.]*)(\.((mp3)|(wav)))*";
    private const string actionPattern = @"Act[1-9]\.*[1-9]*";

    private const string audioFilePath = "Assets/_Modules/Game/_Shared/Sounds";

#if UNITY_EDITOR
    [Button]
    private void Generate()
    {
        GenerateDynamicSoundGroupCreator();
        GenerateSound();
    }

    private void GenerateDynamicSoundGroupCreator()
    {
        var audioFilePaths = new Dictionary<string, string>();
        var nameRegex = new Regex(namePattern);
        foreach (var subDirectory in Directory.GetFileSystemEntries(audioFilePath))
        {
            if (subDirectory.EndsWith(".meta"))
            {
                continue;
            }

            if (Directory.Exists(subDirectory))
            {
                foreach (var file in Directory.GetFiles(subDirectory))
                {
                    if (file.EndsWith(".meta"))
                    {
                        continue;
                    }

                    var audioKey = nameRegex.Match(file).Groups[2].Value.ToLower();
                    if (audioKey.IsNullOrWhitespace())
                    {
                        continue;
                    }

                    if (audioFilePaths.ContainsKey(audioKey))
                    {
                        Debug.LogWarning($"[Duplicate file]: {file}");
                    }
                    else
                    {
                        audioFilePaths.Add(audioKey, file);
                    }
                }
            }
            else
            {
                audioFilePaths.Add(nameRegex.Match(subDirectory).Groups[2].Value.ToLower(), subDirectory);
            }
        }

        var dsgLevel = Instantiate(this.dsgPrefab, this.levelRoot);
        dsgLevel.name = "DSG_Level";
        var events = skeletonAnimation.Skeleton.Data.Events;
        var addedGroupName = new List<string>();
        foreach (var e in events)
        {
            var dynamicSoundGroupName = nameRegex.Match(e.Name).Groups[0].Value;
            if (addedGroupName.Contains(dynamicSoundGroupName))
            {
                continue;
            }

            addedGroupName.Add(dynamicSoundGroupName);
            var soundKey = nameRegex.Match(e.Name).Groups[2].Value.ToLower();
            var dynamicGroup = Instantiate(dsgLevel.groupTemplate, dsgLevel.transform)
                .GetComponent<DynamicSoundGroup>();
            dynamicGroup.name = nameRegex.Match(e.Name).Groups[0].Value;
            dynamicGroup.isExistingBus = true;
            dynamicGroup.busName = "SFX";
            var dynamicGroupVariation = Instantiate(dsgLevel.variationTemplate, dynamicGroup.transform)
                .GetComponent<DynamicGroupVariation>();
            dynamicGroupVariation.name = $"{dynamicGroup.name}1";
            dynamicGroup.busIndex = this.busIndex;
            try
            {
                var guid = AssetDatabase.GUIDFromAssetPath(audioFilePaths[soundKey]);
                // dynamicGroupVariation.audioClipAddressable = new AssetReference(guid.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }

    private void GenerateSound()
    {
        var events = skeletonAnimation.Skeleton.Data.Events;
        var nameRegex = new Regex(namePattern);
        var actionRegex = new Regex(actionPattern);
        var children = GetComponentsInChildren<PlaySoundSpine>();
        var actionGos = new Dictionary<string, GameObject>();
        foreach (var actionGo in GetComponentsInChildren<VisualParallel>())
        {
            actionGos.Add(actionGo.name, actionGo.gameObject);
        }

        foreach (var e in events)
        {
            var goName = $"PlaySoundSpine_{e.Name}";
            PlaySoundSpine playSoundSpine = null;
            foreach (var child in children)
            {
                if (child.name.Equals(goName))
                {
                    playSoundSpine = child;
                    break;
                }
            }

            if (playSoundSpine == null)
            {
                var go = new GameObject(goName);
                go.transform.SetParent(this.transform);
                playSoundSpine = go.AddComponent<PlaySoundSpine>();
                playSoundSpine.SkeletonAnimation = skeletonAnimation;
                playSoundSpine.eventName = e.Name;
            }

            foreach (var soundName in GetSoundGroups())
            {
                if (e.Name.Contains(soundName))
                {
                    playSoundSpine.NameMusic = soundName;
                }
            }

            var actionName = actionRegex.Match(e.Name).Value;
            if (actionGos.TryGetValue(actionName, out var actionGo))
            {
                playSoundSpine.transform.SetParent(actionGo.transform);
            }
            else
            {
                var newActionGo = new GameObject(actionName);
                newActionGo.AddComponent<VisualParallel>();
                newActionGo.transform.SetParent(this.transform);
                actionGos.Add(actionName, newActionGo);
                playSoundSpine.transform.SetParent(newActionGo.transform);
            }
        }
    }

    private static IEnumerable<string> GetSoundGroups()
    {
        return MasterAudio.SafeInstance.GroupNames;
    }
#endif
}