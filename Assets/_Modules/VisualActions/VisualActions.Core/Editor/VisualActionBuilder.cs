using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.Editor
{
    public class VisualActionBuilder
    {
        public static GameObject CreateGameObject(string name, MenuCommand menuCommand)
        {
            var go = new GameObject(name);
            go.transform.position = Vector3.zero;
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, go.GetInstanceID().ToString());
            Selection.activeObject = go;
            return go;
        }
        
        public static GameObject CreateGameObject(string name)
        {
            var go = new GameObject(name);
            go.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(go, go.GetInstanceID().ToString());
            Selection.activeObject = go;
            return go;
        }

        public static T CreateActionGameObject<T>(string name, MenuCommand menuCommand) where T : VisualAction
        {
            GameObject go = CreateGameObject(name, menuCommand);
            return go.AddComponent<T>();
        }

        public static T CreateActionGameObject<T>(string name, MenuCommand menuCommand, GUIStyle nameStyle)
            where T : VisualAction, new()
        {
            GameObject go = CreateGameObject(name, menuCommand);
            var action = go.AddComponent<T>();
            VisualActionStateVisualizer.RegisterActionNameStyle(action.GetType(), nameStyle);
            return action;
        }
    }
}