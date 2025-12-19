using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Mimi.CommandEditor
{
    public class CustomMenuEditor : EditorWindow
    {
        [MenuItem("Mimi/Command Menu")]
        public static void Open() => GetWindow<CustomMenuEditor>("Command Menu").Show();

        private SearchField searchField;
        private string searchItem;
        private TreeNode _root = new TreeNode { Name = "Root" };
        private object _selected;
        private Vector2 _leftScroll, _rightScroll;
        private Color selectedColor = new Color(0.0f, 0.45f, 1.0f, 0.35f);

        private void OnEnable()
        {
            searchField = new SearchField();
            RefreshTree();
        }

        #region === REFRESH ===

        [ContextMenu("Refresh Tree")]
        private void RefreshTree()
        {
            _root.Children.Clear();
            var map = new Dictionary<string, TreeNode>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes().Where(t => t.IsClass && t.IsAbstract && t.IsSealed))
                {
                    foreach (var mi in type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                       BindingFlags.NonPublic))
                    {
                        var attr = mi.GetCustomAttribute<MiCommand>();
                        if (attr == null) continue;
                        if (mi.ReturnType != typeof(void) || mi.GetParameters().Length != 0)
                        {
                            Debug.LogWarning(
                                $"[IMenu] {mi.DeclaringType}.{mi.Name}: must be static void, no parameters.");
                            continue;
                        }

                        var parts = attr.Path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
                        if (parts.Length == 0) continue;

                        TreeNode cur = _root;
                        string curPath = "";

                        for (int i = 0; i < parts.Length - 1; i++)
                        {
                            string p = parts[i];
                            curPath += "/" + p;
                            if (!map.TryGetValue(curPath, out TreeNode child))
                            {
                                child = new TreeNode { Name = p, Parent = cur };
                                cur.Children.Add(child);
                                map[curPath] = child;
                            }

                            cur = child;
                        }

                        var leaf = new MenuItemData
                        {
                            Name = parts[^1],
                            Description = attr.Description,
                            Author = attr.Author,
                            Method = mi,
                            Parent = cur
                        };
                        cur.Leaves.Add(leaf);
                    }
                }
            }

            Sort(_root);
            _selected = null;
            Repaint();
        }

        private void Sort(TreeNode n)
        {
            n.Children = n.Children.OrderBy(c => c.Name).ToList();
            foreach (var c in n.Children)
            {
                Sort(c);
                c.Leaves = c.Leaves.OrderBy(l => l.Name).ToList();
            }
        }

        #endregion

        public bool CheckSearch(TreeNode node)
        {
            node.isMatchSearching = false;
            foreach (var item in node.Children)
            {
                if (CheckSearch(item))
                {
                    node.isMatchSearching = true;
                    node.Foldout = true;
                }
            }

            foreach (var leaf in node.Leaves)
            {
                if (leaf.Name.ToLower().Contains(searchItem.ToLower()) || string.IsNullOrEmpty(searchItem))
                {
                    leaf.isMatchSearching = true;
                    node.isMatchSearching = true;
                    node.Foldout = true;
                }
                else
                {
                    leaf.isMatchSearching = false;
                }
            }

            if (string.IsNullOrEmpty(searchItem) || node.Name.ToLower().Contains(searchItem.ToLower()))
            {
                node.isMatchSearching = true;
            }

            return node.isMatchSearching;
        }


        #region === GUI ===

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSearchField()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            // Hiển thị thanh tìm kiếm bằng SearchField.OnToolbarGUI
            var previous = searchItem;
            searchItem = searchField.OnToolbarGUI(searchItem);
            if (searchItem != previous)
            {
                CheckSearch(_root);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(300), GUILayout.ExpandHeight(true));
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label("Menu Tree", GUILayout.Width(100));
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60))) RefreshTree();
                EditorGUILayout.EndHorizontal();
                DrawSearchField();
                _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll);
                DrawTree(_root, 0);
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTree(TreeNode node, int indent)
        {
            if (!node.isMatchSearching) return;
            if (node == _root)
            {
                foreach (var c in node.Children) DrawTree(c, 0);
                return;
            }

            bool isSelected = _selected == node;
            bool hasContent = node.Children.Count > 0 || node.Leaves.Count > 0;
            string prefix = hasContent ? (node.Foldout ? "📂" : "📁") : "  ";

            // === LẤY RECT FULL DÒNG ===
            Rect fullRect = EditorGUILayout.GetControlRect(false, 20);
            fullRect.xMin += indent * 15;

            // === HIGHLIGHT FULL LINE ===
            if (isSelected)
            {
                EditorGUI.DrawRect(fullRect, selectedColor); // xanh sáng
            }

            // === BUTTON VỚI EMOJI ===
            if (GUI.Button(fullRect, prefix + node.Name, EditorStyles.label))
            {
                if (hasContent) node.Foldout = !node.Foldout;
                _selected = node;
            }

            if (!node.Foldout) return;

            foreach (var c in node.Children) DrawTree(c, indent + 1);

            foreach (var leaf in node.Leaves)
            {
                if (!leaf.isMatchSearching) continue;
                Rect leafRect = EditorGUILayout.GetControlRect(false, 20);
                leafRect.xMin += (indent + 1) * 15;

                bool leafSelected = _selected == leaf;
                if (leafSelected)
                {
                    EditorGUI.DrawRect(leafRect, selectedColor);
                }

                if (GUI.Button(leafRect, "⚙️" + leaf.Name, EditorStyles.label))
                    _selected = leaf;
            }
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);

                if (_selected is MenuItemData item)
                {
                    EditorGUILayout.Space(15);
                    var titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 16,
                        fontStyle = FontStyle.Bold
                    };
                    EditorGUILayout.LabelField(item.Name, titleStyle);
                    var authorStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontSize = 11,
                        fontStyle = FontStyle.Italic,
                        normal = { textColor = Color.gray }
                    };
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField($"From {item.Author}", authorStyle);
                    EditorGUILayout.Space(12);

                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);

// Tạo style giống text box Unity
                        var boxStyle = new GUIStyle(EditorStyles.textArea)
                        {
                            wordWrap = true,
                            normal =
                            {
                                textColor = Color.white,
                                background = Texture2D.linearGrayTexture
                            },
                            fontSize = 12,
                            padding = new RectOffset(6, 6, 6, 6)
                        };
                        Vector2 _scrollDesc = Vector2.zero;
// Bao quanh bằng scroll view nếu mô tả dài
                        _scrollDesc = EditorGUILayout.BeginScrollView(_scrollDesc, GUILayout.MinHeight(60));
                        EditorGUILayout.SelectableLabel(item.Description, boxStyle, GUILayout.ExpandHeight(true));
                        EditorGUILayout.EndScrollView();

                        EditorGUILayout.Space(15);
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Execute", GUILayout.Width(80), GUILayout.Height(24)))
                    {
                        try
                        {
                            item.Method.Invoke(null, null);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Execute failed: {e}");
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(20);
                }
                else if (_selected is TreeNode folder)
                {
                    EditorGUILayout.Space(15);
                    EditorGUILayout.LabelField(folder.Name, EditorStyles.largeLabel);
                    EditorGUILayout.LabelField("Folder", EditorStyles.miniLabel);
                    EditorGUILayout.HelpBox("This is a folder. Select a command to execute.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Select a command from the left to view details.", MessageType.Info);
                }

                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}