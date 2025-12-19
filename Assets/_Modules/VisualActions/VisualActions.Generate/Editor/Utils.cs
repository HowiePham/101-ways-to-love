using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor
{
    public static class Utils
    {
        public static string GetRootPath()
        {
            // Lấy đường dẫn tuyệt đối tới script hiện tại
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(ScriptableObject.CreateInstance<ScriptableObject>()));
            Debug.Log(scriptPath);
            string editorDir = Path.GetDirectoryName(scriptPath);
            string packageRoot = Directory.GetParent(editorDir).FullName;

            // Đường dẫn tuyệt đối tới prefab
            
            if (packageRoot.Contains("/Packages/"))
            {
                packageRoot = packageRoot.Substring(packageRoot.IndexOf("Packages/"));
            }
            else if (packageRoot.Contains("/Assets/"))
            {
                packageRoot = packageRoot.Substring(packageRoot.IndexOf("Assets/"));
            }

            return packageRoot;
        }

        public static void SetFieldObject(object obj,Type type, string fieldName, object value)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }
    }
}