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
        Transform levelSoundParent = this.levelEditor.GeneralLevelSound;
        PlayAudio[] levelSounds = levelSoundParent.GetComponentsInChildren<PlayAudio>();

        foreach (PlayAudio playAudio in levelSounds)
        {
            EditorGUILayout.LabelField($"{playAudio.gameObject.name}", EditorStyles.boldLabel);
            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = playAudio.gameObject;
                EditorGUIUtility.PingObject(playAudio.gameObject);
            }
            // EditorGUILayout.BeginHorizontal();
            // var serializedObject = new SerializedObject(playAudio);
            // SerializedProperty soundKeyProp = serializedObject.FindProperty("soundKey");
            // EditorGUILayout.LabelField("Sound Key:", GUILayout.Width(100));
            // EditorGUILayout.PropertyField(soundKeyProp, GUIContent.none);
            // EditorGUILayout.EndHorizontal();

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