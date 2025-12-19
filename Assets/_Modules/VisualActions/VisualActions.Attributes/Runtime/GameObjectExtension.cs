using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Mimi.VisualActions.Attribute
{
    public static class GameObjectExtension
    {
        public static void FetchDependency(this GameObject currentGameObject, GameObject other)
        {

     var components = currentGameObject.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var comp in components)
        {
            var type = comp.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (!field.IsDefined(typeof(MainInputAttribute), true)) continue;

                Type fieldType = field.FieldType;
                string log = $"{type.Name}.{field.Name}";

                // 1. GameObject[] → GÁN NGAY other (root)
                if (fieldType == typeof(GameObject[]))
                {
                    field.SetValue(comp, new[] { other });
                    Debug.Log($"<color=cyan>[MainInput]</color> <b>GameObject[]:</b> <color=lime>Root</color> → <color=yellow>{log}</color>");
                    continue;
                }

                // 2. List<GameObject> → GÁN NGAY other
                if (fieldType == typeof(List<GameObject>))
                {
                    field.SetValue(comp, new List<GameObject> { other });
                    Debug.Log($"<color=cyan>[MainInput]</color> <b>List<GameObject>:</b> <color=lime>Root</color> → <color=yellow>{log}</color>");
                    continue;
                }

                // 3. GameObject đơn → GÁN other
                if (fieldType == typeof(GameObject))
                {
                    field.SetValue(comp, other);
                    Debug.Log($"<color=cyan>[MainInput]</color> <b>GameObject:</b> <color=lime>Root</color> → <color=yellow>{log}</color>");
                    continue;
                }

                // 4. Component đơn
                if (typeof(Component).IsAssignableFrom(fieldType) && !fieldType.IsArray && 
                    !(fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var found = other.GetComponentInChildren(fieldType, true);
                    if (found)
                    {
                        field.SetValue(comp, found);
                        Debug.Log($"<color=cyan>[MainInput]</color> <b>Đơn:</b> <color=lime>{fieldType.Name}</color> → <color=yellow>{log}</color>");
                    }
                    else
                    {
                        Debug.LogWarning($"<color=orange>[MainInput]</color> Không tìm thấy <color=red>{fieldType.Name}</color> tại <color=yellow>{log}</color>");
                    }
                    continue;
                }

                // 5. Mảng Component[] → chỉ 1 phần tử đầu
                if (fieldType.IsArray && typeof(Component).IsAssignableFrom(fieldType.GetElementType()))
                {
                    Type elem = fieldType.GetElementType();
                    var first = other.GetComponentInChildren(elem, true);
                    if (first)
                    {
                        var arr = Array.CreateInstance(elem, 1);
                        arr.SetValue(first, 0);
                        field.SetValue(comp, arr);
                        Debug.Log($"<color=cyan>[MainInput]</color> <b>Mảng[1]:</b> <color=lime>{elem.Name}</color> → <color=yellow>{log}</color>");
                    }
                    continue;
                }

                // 6. List<Component> → chỉ 1 phần tử đầu
                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type elem = fieldType.GetGenericArguments()[0];
                    if (typeof(Component).IsAssignableFrom(elem))
                    {
                        var first = other.GetComponentInChildren(elem, true);
                        if (first)
                        {
                            var list = Activator.CreateInstance(fieldType);
                            fieldType.GetMethod("Add").Invoke(list, new[] { first });
                            field.SetValue(comp, list);
                            Debug.Log($"<color=cyan>[MainInput]</color> <b>List[1]:</b> <color=lime>{elem.Name}</color> → <color=yellow>{log}</color>");
                        }
                    }
                    continue;
                }
            }
        }

        Debug.Log("<color=green><b>[MainInput] HOÀN TẤT – GameObject mảng = Root!</b></color>");
        }
    }
}