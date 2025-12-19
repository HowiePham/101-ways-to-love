using System.Linq;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Dragging;
using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor.DragExtension
{
     public abstract class BaseDragExtensionEdit<T> : IDragExtensionEdit<T> where T : MonoDragExtension
    {
        [SerializeField, HideInInspector]
        private bool isToggle;
        
        [SerializeReference]
        private T extension;

        [SerializeField, HideInInspector]
        private VisualDrag visualDrag;
        
        [SerializeField, HideInInspector]
        private CompositeDragExtension compositeDragExtension;
        
        public T Extension => extension;
        public VisualDrag VisualDrag => visualDrag;
        public  bool IsToggle
        {
            get => isToggle;
            set
            {
                if (isToggle == value) return;
                isToggle = value;
                if (isToggle) TurnOnHandle();
                else TurnOffHandle();
            }
        }

        protected abstract T CreateNewExtension();
        public virtual void TurnOnHandle()
        {
            if (this.compositeDragExtension != null)
            {
                if (this.extension == null)
                {
                    this.extension = CreateNewExtension();
                    this.extension.transform.SetParent(compositeDragExtension.transform);
                }
                var field = this.compositeDragExtension
                    .GetFieldValue<MonoDragExtension[]>("extensions", AccessModifier.Private).ToList();
                field.Add(extension);
                this.compositeDragExtension.SetField("extensions", field.ToArray(), AccessModifier.Private);
            }
            EditorUtility.SetDirty(this.compositeDragExtension);
        }

        public virtual void TurnOffHandle()
        {
            if (this.compositeDragExtension != null && this.extension != null)
            {
                var field = this.compositeDragExtension
                    .GetFieldValue<MonoDragExtension[]>("extensions", AccessModifier.Private).ToList();
                field.Remove(extension);
                this.compositeDragExtension.SetField("extensions", field.ToArray(), AccessModifier.Private);
            }
            EditorUtility.SetDirty(this.compositeDragExtension);
        }
        

        public void SetVisualDrag(VisualDrag visualDrag)
        {
            this.visualDrag = visualDrag;
            this.compositeDragExtension = this.visualDrag.GetComponentInChildren<CompositeDragExtension>();
        }
    }
}