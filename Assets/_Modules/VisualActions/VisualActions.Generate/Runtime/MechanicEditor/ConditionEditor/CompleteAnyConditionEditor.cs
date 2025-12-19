using System;
using System.Collections.Generic;
using System.Linq;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Generate.Editor;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    [Serializable]
    public class CompletedAnyConditionEditor : ConditionEditor
    {
        [OnAddItem("OnAddCondition")]
        [OnRemoveItem("OnRemoveCondition")]
        [SerializeReference] private List<ConditionEditor> childs = new List<ConditionEditor>();

        private void OnAddCondition(ConditionEditor conditionEditor)
        {
            List<VisualCondition> conditionGenerates = (this.conditionObject as CompleteAnyCondition).Conditions.ToList();
            conditionEditor.OnInit(conditionObject.gameObject);
            conditionGenerates.Add(conditionEditor.conditionObject);
            this.conditionObject.SetField("conditions", conditionGenerates.ToArray(), AccessModifier.Private);
        }

        private void OnRemoveCondition(ConditionEditor conditionEditor)
        {
            List<VisualCondition> conditionGenerates = (this.conditionObject as CompleteAnyCondition).Conditions.ToList();
            conditionGenerates.Remove(conditionEditor.conditionObject);
            this.conditionObject.SetField("conditions", conditionGenerates.ToArray(), AccessModifier.Private);
            conditionEditor.OnRemove();
        }
        public override void OnInit(GameObject root)
        {
            this.conditionObject = new CompleteAnyConditionGenerator().Generate();
            this.conditionObject.transform.SetParent(root.transform);
           
        }
    }
}