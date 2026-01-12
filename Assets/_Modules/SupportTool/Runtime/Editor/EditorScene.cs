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
    
    [MenuItem("Open_Scene/XepLevel")]
    static void OpenXepLevel()
    {
        EditorSceneManager.OpenScene("Assets/_Modules/_Scenes/DevScene/XepLevel.unity");
    }
}
#endif
