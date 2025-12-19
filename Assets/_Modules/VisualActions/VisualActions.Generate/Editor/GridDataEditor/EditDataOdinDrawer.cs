using System.Reflection;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Generate.Editor;
using UnityEditor;

namespace Mimi.VisualActions.Generate.Editor
{

#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

public class EditDataDrawer : OdinAttributeDrawer<EditDataAttribute, List<IDataEdit>>
{
    private Dictionary<object, PropertyTree> cachePropertyTrees = new Dictionary<object, PropertyTree>();
    private void DrawAllSerializedFields(object element, InspectorProperty elementProp, float maxWidth)
    {
        float fieldWidth = maxWidth - 32f;

        foreach (var child in elementProp.Children)
        {
            if (child.Name == "isToggle") continue;
            if (child.Info.GetAttribute<HideInInspector>() != null) continue;
            if (child.Info.GetAttribute<HideInBehaviourEditorAttribute>() != null) continue;

            var processorObj = child.ValueEntry.WeakSmartValue;
            if (processorObj == null) continue;

            if (cachePropertyTrees.TryGetValue(processorObj, out var oldTree))
            {
                var oldTarget = oldTree.WeakTargets.FirstOrDefault();
                if (!ReferenceEquals(oldTarget, processorObj))
                {
                    oldTree.Dispose();
                    cachePropertyTrees.Remove(processorObj);
                }
            }

            if (!cachePropertyTrees.TryGetValue(processorObj, out var tree))
            {
                tree = PropertyTree.Create(processorObj);
                cachePropertyTrees[processorObj] = tree;
            }

            tree.UpdateTree();
            tree.BeginDraw(false);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * 15f);
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(fieldWidth));
                {
                    foreach (var subProp in tree.EnumerateTree(false))
                    {
                        if (subProp.Info.GetAttribute<HideInBehaviourEditorAttribute>() != null) continue;
                        subProp.Draw();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            tree.EndDraw();
            EditorGUILayout.EndHorizontal();
        }
    }

    private bool showBox = true;
    private Vector2 gridScrollPos = Vector2.zero;
    private string searchFilter = ""; // ← Biến lưu trạng thái search

    protected override void DrawPropertyLayout(GUIContent label)
    {
        showBox = EditorGUILayout.Foldout(showBox, label, true);

        if (!showBox)
            return;
        var interfaceType = Attribute.InterfaceType;

        if (ValueEntry.SmartValue == null)
            ValueEntry.SmartValue = new List<IDataEdit>();

        var list = ValueEntry.SmartValue;
        AutoPopulateImplementations(interfaceType, list);


        SirenixEditorGUI.BeginBox();
        {
            SirenixEditorGUI.HorizontalLineSeparator();

            // === THANH SEARCH BAR ===
            DrawSearchBar();

            // === FILTER LIST THEO SEARCH ===
            var filteredList = string.IsNullOrEmpty(searchFilter)
                ? list
                : list.Where(item => item != null &&
                                     item.GetType().Name.ToLower().Contains(searchFilter.ToLower())).ToList();

            if (filteredList.Count == 0 && list.Count > 0)
            {
                GUILayout.Label("No behaviors match search", SirenixGUIStyles.LabelCentered);
            }
            else
            {
                DrawResponsiveGrid(filteredList);
            }
        }
        SirenixEditorGUI.EndBox();
    }
    private void DrawSearchBar()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Search", GUILayout.Width(50));
            // SỬA: Dùng EditorGUILayout.TextField + Odin style
            searchFilter = EditorGUILayout.TextField(
                searchFilter, 
                SirenixGUIStyles.ToolbarSearchTextField,  // Giữ style Odin
                GUILayout.Height(18f), 
                GUILayout.ExpandWidth(true)    // Mở rộng để tránh rect cố định
            );
            
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawResponsiveGrid(List<IDataEdit> items)
    {
        const float PADDING = 8f;
        const float MIN_GROUP_WIDTH = 250f;
        const float MAX_GROUP_WIDTH = 400f;
        const float VERTICAL_SPACING = 8f;
        const float MAX_GRID_HEIGHT = 300f;

        float inspectorWidth = Mathf.Max(EditorGUIUtility.currentViewWidth - 40f, 100f);
        int maxCols = Mathf.FloorToInt((inspectorWidth + PADDING) / (MIN_GROUP_WIDTH + PADDING));
        int columns = Mathf.Clamp(maxCols, 1, 2);

        float totalPadding = PADDING * (columns - 1);
        float available = inspectorWidth - totalPadding;
        float groupWidth = Mathf.Clamp(available / columns, MIN_GROUP_WIDTH, MAX_GROUP_WIDTH);

        // CHỈ SCROLL DỌC
        gridScrollPos = EditorGUILayout.BeginScrollView(
            gridScrollPos,
            false,  // Không scroll ngang
            true,   // Có scroll dọc
            GUILayout.Height(MAX_GRID_HEIGHT)
        );

        int index = 0;
        while (index < items.Count)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < columns && index < items.Count; c++, index++)
            {
                var element = items[index];
                var elementProp = Property.Children[index];
                DrawGroup(element, elementProp, groupWidth);
                if (c < columns - 1) GUILayout.Space(PADDING);
            }
            EditorGUILayout.EndHorizontal();
            if (index < items.Count) EditorGUILayout.Space(VERTICAL_SPACING);
        }

        EditorGUILayout.EndScrollView();
    }
    
    private void DrawGroup(IDataEdit element, InspectorProperty elementProp, float groupWidth)
    {
        bool isToggle = element.IsToggle;
        string className = element.GetType().Name;

        // Đổi màu nút theo trạng thái toggle
        GUI.backgroundColor = isToggle
            ? new Color(0.3f, 0.65f, 0.65f, 1f)
            : new Color(0.65f, 0.65f, 0.65f, 1f);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(groupWidth));
        {
            // Nút toggle
            if (GUILayout.Button(className, GUILayout.Height(30)))
            {
                element.IsToggle = !element.IsToggle;
                GUI.FocusControl(null); // 🔥 Giải phóng focus, tránh "nuốt" event click khác
            }

            GUI.backgroundColor = Color.white;

            bool visible = element.IsToggle;
            var fadeKey = element.GetHashCode();

            bool fadeGroupOpen = SirenixEditorGUI.BeginFadeGroup(this, fadeKey, visible);
            if (fadeGroupOpen)
            {
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins,GUILayout.Width(groupWidth));

                // Vẽ field
                DrawAllSerializedFields(element, elementProp,groupWidth);
                
                EditorGUILayout.EndVertical();
            }
            SirenixEditorGUI.EndFadeGroup();
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Tự động quét và thêm toàn bộ class con kế thừa từ interfaceType.
    /// </summary>
    private void AutoPopulateImplementations(Type interfaceType, List<IDataEdit> list)
    {
        // Lấy tất cả type implement interfaceType
        var types = TypeCache.GetTypesDerivedFrom(interfaceType)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .OrderBy(t => t.Name)
            .ToList();

        bool changed = false;

        foreach (var t in types)
        {
            bool exists = list.Any(e => e != null && e.GetType() == t);
            if (!exists)
            {
                var instance = Activator.CreateInstance(t) as IDataEdit;
                list.Add(instance);
                changed = true;
            }
        }

        // Xóa phần tử nào không còn tồn tại trong code (vd class bị xóa)
        list.RemoveAll(e => e == null || !types.Contains(e.GetType()));

        if (changed)
        {
            GUI.changed = true;
        }
    }
}
#endif



}