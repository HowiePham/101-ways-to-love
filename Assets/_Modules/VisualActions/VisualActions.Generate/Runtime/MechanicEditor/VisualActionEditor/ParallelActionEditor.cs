using System.Collections.Generic;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Generate.Editor;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    public class ParallelActionEditor : ActionEditor
    {
        [OnAddItem("OnAddAction")]
        [OnRemoveItem("OnRemoveAction")]
        [SerializeReference] private List<ActionEditor> childs = new List<ActionEditor>();

        private void OnAddAction(ActionEditor actionEditor)
        {
            actionEditor.OnInit(actionObject.gameObject);
        }

        private void OnRemoveAction(ActionEditor actionEditor)
        {
            actionEditor.OnRemove();
        }
        public override void OnInit(GameObject root)
        {
            this.actionObject = new ParallelGenerator().Generate();
            this.actionObject.transform.SetParent(root.transform);
        }
    }
}