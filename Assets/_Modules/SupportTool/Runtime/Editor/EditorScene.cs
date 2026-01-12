#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EditorScene : MonoBehaviour
{
    [MenuItem("Open_Scene/Loading")]
    static void OpenLoading()
    {
        EditorSceneManager.OpenScene("Assets/_Modules/_Scenes/BootScene/Scene_Boot.unity");
    }

    [MenuItem("Open_Scene/GamePlay")]
    static void OpenGamePlay()
    {
        EditorSceneManager.OpenScene("Assets/_Modules/_Scenes/GameScene/Scene_Game.unity");
    }

    [MenuItem("Open_Scene/XepLevel_Dev")]
    static void OpenXepLevelDev()
    {
        EditorSceneManager.OpenScene("Assets/_Modules/_Scenes/DevScene/XepLevel_Dev.unity");
    }

    [MenuItem("Open_Scene/XepLevel_QA")]
    static void OpenXepLevelQA()
    {
        EditorSceneManager.OpenScene("Assets/_Modules/_Scenes/DevScene/XepLevel_QA.unity");
    }

    [MenuItem("Open_Scene/XepLevel_GD")]
    static void OpenXepLevelGD()
    {
        EditorSceneManager.OpenScene("Assets/_Modules/_Scenes/DevScene/XepLevel_GD.unity");
    }

    [MenuItem("Open_Scene/XepLevel_Art")]
    static void OpenXepLevelArt()
    {
        EditorSceneManager.OpenScene("Assets/_Modules/_Scenes/DevScene/XepLevel_Art.unity");
    }

    [MenuItem("Open_Scene/XepLevel_Anim")]
    static void OpenXepLevelAnim()
    {
        EditorSceneManager.OpenScene("Assets/_Modules/_Scenes/DevScene/XepLevel_Anim.unity");
    }
}
#endif