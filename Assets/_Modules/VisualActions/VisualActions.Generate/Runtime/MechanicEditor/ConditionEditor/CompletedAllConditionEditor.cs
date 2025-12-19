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
    public class CompletedAllConditionEditor : ConditionEditor
    {
        [OnAddItem("OnAddCondition")]
        [OnRemoveItem("OnRemoveCondition")]
        [SerializeReference] private List<ConditionEditor> childs = new List<ConditionEditor>();
        public override void OnInit(GameObject root)
        {
            this.conditionObject = new CompleteAllConditionGenerator().Generate();
            this.conditionObject.transform.SetParent(root.transform);
            List<VisualCondition> conditionGenerates = new List<VisualCondition>();
            foreach (var condition in childs)
            {
                condition.OnInit(conditionObject.gameObject);
                conditionGenerates.Add(condition.conditionObject);
            }
            this.conditionObject.SetField("conditions", conditionGenerates.ToArray(), AccessModifier.Private);
        }
        private void OnAddCondition(ConditionEditor conditionEditor)
        {
            List<VisualCondition> conditionGenerates = (this.conditionObject as CompleteAllCondition).Conditions.ToList();
            conditionEditor.OnInit(conditionObject.gameObject);
            conditionGenerates.Add(conditionEditor.conditionObject);
            this.conditionObject.SetField("conditions", conditionGenerates.ToArray(), AccessModifier.Private);
        }

        private void OnRemoveCondition(ConditionEditor conditionEditor)
        {
            List<VisualCondition> conditionGenerates = (this.conditionObject as CompleteAllCondition).Conditions.ToList();
            conditionGenerates.Remove(conditionEditor.conditionObject);
            this.conditionObject.SetField("conditions", conditionGenerates.ToArray(), AccessModifier.Private);
            conditionEditor.OnRemove();
        }
    }
}