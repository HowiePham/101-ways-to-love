using Mimi.VisualActions.Spines;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

public class SkeletonAnimationEditorSection : EditorSection
{
    public SkeletonAnimationEditorSection(LevelEditor levelEditor) : base(levelEditor)
    {
    }

    public override void DrawSection()
    {
        EditorGUILayout.BeginHorizontal();

        SkeletonAnimation skeletonAnimation = this.levelEditor.SkeletonAnimation;
        DrawSkeletonAnimationSection(skeletonAnimation);

        if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
        {
            Selection.activeGameObject = skeletonAnimation.gameObject;
            EditorGUIUtility.PingObject(skeletonAnimation.gameObject);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Order In Layer:", GUILayout.Width(60));

        var renderer = skeletonAnimation.GetComponent<MeshRenderer>();
        EditorGUILayout.BeginHorizontal();
        DrawSortingLayerSection(renderer);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawOrderInLayerSection(renderer);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        SpineAnimMechanic[] spineAnimMechanics = this.levelEditor.SpineAnimMechanics;
        EditorGUI.indentLevel++;

        for (int i = 0; i < spineAnimMechanics.Length; i++)
        {
            SpineAnimMechanic animMechanic = spineAnimMechanics[i];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i + 1}. {animMechanic.gameObject.name}", EditorStyles.boldLabel);

            var newAnim = (SkeletonAnimation)EditorGUILayout.ObjectField(
                animMechanic.SkeletonAnimation,
                typeof(SkeletonAnimation),
                true,
                GUILayout.Height(18)
            );

            if (newAnim != animMechanic.SkeletonAnimation)
            {
                Undo.RecordObject(animMechanic, "Change Spine Animation");
                animMechanic.SkeletonAnimation = newAnim;
                EditorUtility.SetDirty(animMechanic);
            }

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = animMechanic.gameObject;
                EditorGUIUtility.PingObject(animMechanic.gameObject);
            }

            EditorGUILayout.EndHorizontal();

            var serializedObject = new SerializedObject(animMechanic);
            SerializedProperty animProp = serializedObject.FindProperty("animation");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Action:", GUILayout.Width(50));
            EditorGUILayout.PropertyField(animProp, GUIContent.none, true);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.indentLevel--;
    }
}