using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Audio;
using UnityEditor;
using UnityEngine;

public class SoundEditorSection : EditorSection
{
    public SoundEditorSection(LevelEditor levelEditor) : base(levelEditor)
    {
    }

    public override void DrawSection()
    {
        PlayAudio[] levelSounds = this.levelEditor.LevelSounds;

        for (var i = 0; i < levelSounds.Length; i++)
        {
            PlayAudio playAudio = levelSounds[i];
            EditorGUILayout.LabelField($"{playAudio.gameObject.name}", EditorStyles.boldLabel);
            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = playAudio.gameObject;
                EditorGUIUtility.PingObject(playAudio.gameObject);
            }

            DrawVolumeSection(playAudio);
            DrawPitchSection(playAudio);

            EditorGUILayout.Space(10);
        }
    }

    private void DrawVolumeSection(PlayAudio playAudio)
    {
        EditorGUILayout.LabelField("Volume:");
        float currentVolume = playAudio.Volume;
        EditorGUI.BeginChangeCheck();
        float newVolume = EditorGUILayout.Slider(currentVolume, 0, 1);
        if (EditorGUI.EndChangeCheck())
        {
            playAudio.SetField("volume", newVolume, AccessModifier.Private);
        }
    }

    private void DrawPitchSection(PlayAudio playAudio)
    {
        EditorGUILayout.LabelField("Pitch:");
        float currentPitch = playAudio.Pitch;
        EditorGUI.BeginChangeCheck();
        float newPitch = EditorGUILayout.Slider(currentPitch, 0, 1);
        if (EditorGUI.EndChangeCheck())
        {
            playAudio.SetField("pitch", newPitch, AccessModifier.Private);
        }
    }
}