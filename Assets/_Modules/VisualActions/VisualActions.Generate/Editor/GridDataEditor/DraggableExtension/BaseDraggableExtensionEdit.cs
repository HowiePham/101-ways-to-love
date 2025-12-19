using System.Collections.Generic;
using System.Linq;
using Mimi.Interactions;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.Reflection.Extensions;
using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public abstract class BaseDraggableExtensionEdit<T> : IDraggableExtensionEdit<T> where T : MonoDraggableExtension
    {
        [SerializeField, HideInInspector]
        private bool isToggle;
        
        [SerializeReference]
        private T extension;

        [SerializeField, HideInInspector]
        private BaseDraggable draggable;
        
        [SerializeField, HideInInspector]
        private CompositeDraggableExtension compositeDraggableExtension;
        
        public T Extension => extension;
        public BaseDraggable Draggable => draggable;
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
            if (this.compositeDraggableExtension != null)
            {
                if (this.extension == null)
                {
                    this.extension = CreateNewExtension();
                    this.extension.transform.SetParent(this.compositeDraggableExtension.transform);
                    this.extension.transform.localPosition = Vector3.zero;
                }
                var field = this.compositeDraggableExtension
                    .GetFieldValue<MonoDraggableExtension[]>("draggableExtensions", AccessModifier.Private).ToList();
                field.Add(extension);
                this.compositeDraggableExtension.SetField("draggableExtensions", field.ToArray(), AccessModifier.Private);
            }
            EditorUtility.SetDirty(this.compositeDraggableExtension);
        }

        public virtual void TurnOffHandle()
        {
            if (this.compositeDraggableExtension != null && this.extension != null)
            {
                var field = this.compositeDraggableExtension
                    .GetFieldValue<MonoDraggableExtension[]>("draggableExtensions", AccessModifier.Private).ToList();
                field.Remove(extension);
                this.compositeDraggableExtension.SetField("draggableExtensions", field.ToArray(), AccessModifier.Private);
            }
            EditorUtility.SetDirty(this.compositeDraggableExtension);
        }


        public void SetDraggable(BaseDraggable draggable)
        {
            this.draggable = draggable;
            this.compositeDraggableExtension = draggable.GetComponentInChildren<CompositeDraggableExtension>();
            List<MonoDraggableExtension> allExtension = new List<MonoDraggableExtension>();
            for (int i = 0; i < this.compositeDraggableExtension.transform.childCount; i++)
            {
                allExtension.AddRange(this.compositeDraggableExtension.transform.GetChild(i).GetComponents<MonoDraggableExtension>());
            }
            var activeExtensions =  this.compositeDraggableExtension
                .GetFieldValue<MonoDraggableExtension[]>("draggableExtensions", AccessModifier.Private).ToList();
            bool isChangeValue = false;
            foreach (var exten in allExtension)
            {
                if (exten is T newProcessor)
                {
                    this.extension = newProcessor;
                    isChangeValue = true;
                    if (activeExtensions.Contains(exten))
                    {
                        isToggle = true;
                    }
                    else
                    {
                        isToggle = false;
                    }
                    break;
                }
            }

            if (!isChangeValue)
            {
                this.extension = null;
                isToggle = false;
            }
        }
    }
}