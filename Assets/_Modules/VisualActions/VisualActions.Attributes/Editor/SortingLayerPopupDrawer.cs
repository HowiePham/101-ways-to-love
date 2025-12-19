using System;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
namespace Mimi.VisualActions.Attribute.Editor
{



#if UNITY_EDITOR
// Drawer cho string (lưu Sorting Layer Name)
    public class SortingLayerPopupStringDrawer : OdinAttributeDrawer<SortingLayerPopupAttribute, string>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var layers = SortingLayer.layers;
            string[] names = new string[layers.Length];
            int[] ids = new int[layers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                names[i] = layers[i].name;
                ids[i] = layers[i].id;
            }

            int currentIndex = Array.FindIndex(names, n => n == this.ValueEntry.SmartValue);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup(label ?? GUIContent.none, currentIndex, names);
            this.ValueEntry.SmartValue = names[newIndex];
        }
    }

// Drawer cho int (lưu Sorting Layer ID)
    public class SortingLayerPopupIntDrawer : OdinAttributeDrawer<SortingLayerPopupAttribute, int>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var layers = SortingLayer.layers;
            string[] names = new string[layers.Length];
            int[] ids = new int[layers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                names[i] = layers[i].name;
                ids[i] = layers[i].id;
            }

            int currentIndex = Array.FindIndex(ids, id => id == this.ValueEntry.SmartValue);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup(label ?? GUIContent.none, currentIndex, names);
            this.ValueEntry.SmartValue = ids[newIndex];
        }
    }
#endif
}