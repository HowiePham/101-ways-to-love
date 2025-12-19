using System.Collections.Generic;
using System.Linq;
using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Dragging;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Generate.Editor.DragExtension;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    public class VisualDragEditor : VisualMechanicEditor
    {
        [SerializeField, HideInInspector] private VisualToolEditor _toolEditor;
        
        [PropertyOrder(-11)]
        [SerializeField, ReadOnly] private VisualDrag visualDrag;
        
        [ShowInInspector]
        [PropertyOrder(-10)]
        private bool IsCompleteWhenReleaseFinger
        {
            get
            {
                if (visualDrag != null)
                {
                    return !visualDrag.GetFieldValue<bool>("isCheckCompleteWhenDrag", AccessModifier.Private);
                }

                return false;
            }
            set
            {
                if (visualDrag != null)
                {
                    visualDrag.SetField("isCheckCompleteWhenDrag", !value, AccessModifier.Private);
                }
            }
        }

        public BaseDraggable Draggable => _toolEditor.GetDraggable();

        #region Conditions
        [SerializeField, HideInInspector] private CompleteAllCondition targetCondition;
        [OnAddItem("OnAddCondition")]
        [OnRemoveItem("OnRemoveCondition")]
        [SerializeReference] private List<ConditionEditor> conditions = new List<ConditionEditor>();

        #endregion
        [EditData(typeof(IDragExtensionEdit))]
        [SerializeReference] private List<IDataEdit> dragExtensions = new List<IDataEdit>();
        #if UNITY_EDITOR
        [Button]
        public void Refresh()
        {
            _toolEditor = GetComponent<VisualToolEditor>();
            visualDrag = GetComponentInChildren<VisualDrag>();
            targetCondition = GetComponentInChildren<CompleteAllCondition>();
            foreach (var extension in dragExtensions)
            {
                (extension as IDragExtensionEdit).SetVisualDrag(visualDrag);
            }
        }

        public void OnAddCondition(ConditionEditor conditionEditor)
        {
            conditionEditor.OnInit(targetCondition.gameObject);
            var listCondtions = targetCondition.Conditions.ToList();
            listCondtions.Add(conditionEditor.conditionObject);
            targetCondition.SetField("conditions", listCondtions.ToArray(), AccessModifier.Private);
        }

        public void OnRemoveCondition(ConditionEditor conditionEditor)
        {
            var listCondtions = targetCondition.Conditions.ToList();
            listCondtions.Remove(conditionEditor.conditionObject);
            targetCondition.SetField("conditions", listCondtions.ToArray(), AccessModifier.Private);
            conditionEditor.OnRemove();
        }
        #endif
    }
}