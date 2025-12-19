using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mimi.CommandEditor
{
    [Serializable]
    public class MenuItemData : TreeNode
    {
        public string Name;
        public string Description;
        public string Author;
        public MethodInfo Method;
    }

    [Serializable]
    public class TreeNode
    {
        public string Name;
        public List<TreeNode> Children = new List<TreeNode>();
        public List<MenuItemData> Leaves = new List<MenuItemData>();
        public bool Foldout;
        public TreeNode Parent;
        public bool isMatchSearching = true;
    }
}