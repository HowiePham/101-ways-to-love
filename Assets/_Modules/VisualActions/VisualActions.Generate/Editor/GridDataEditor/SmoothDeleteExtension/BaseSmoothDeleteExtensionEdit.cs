using System.Linq;
using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Deleting;
using Mimi.VisualActions.Dragging;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mimi.VisualActions.Generate.Editor.SmoothDeleteExtension
{
     public abstract class BaseSmoothDeleteExtensionEdit<T> : ISmoothDeleteEdit<T> where T : MonoSmoothDeleteExtension
    {
        [SerializeField, HideInInspector]
        private bool isToggle;
        
        [SerializeReference]
        private T extension;

       [SerializeField, HideInInspector]
        protected VisualSmoothDelete visualSmoothDelete;
        [SerializeField, HideInInspector]
        protected BaseDraggable baseDraggable;
        
        [SerializeField, HideInInspector]
        private CompositeSmoothDeleteExtension compositeSmoothDeleteExtension;
        
        public T Extension => extension;
        public VisualSmoothDelete VisualSmoothDelete => visualSmoothDelete;
        
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
            if (this.compositeSmoothDeleteExtension != null)
            {
                if (this.extension == null)
                {
                    this.extension = CreateNewExtension();
                    this.extension.transform.SetParent(compositeSmoothDeleteExtension.transform);
                }
                var field = this.compositeSmoothDeleteExtension
                    .GetFieldValue<MonoSmoothDeleteExtension[]>("extensions", AccessModifier.Private).ToList();
                field.Add(extension);
                this.compositeSmoothDeleteExtension.SetField("extensions", field.ToArray(), AccessModifier.Private);
            }
            EditorUtility.SetDirty(this.compositeSmoothDeleteExtension);
        }

        public virtual void TurnOffHandle()
        {
            if (this.compositeSmoothDeleteExtension != null && this.extension != null)
            {
                var field = this.compositeSmoothDeleteExtension
                    .GetFieldValue<MonoSmoothDeleteExtension[]>("extensions", AccessModifier.Private).ToList();
                field.Remove(extension);
                this.compositeSmoothDeleteExtension.SetField("extensions", field.ToArray(), AccessModifier.Private);
            }
            EditorUtility.SetDirty(this.compositeSmoothDeleteExtension);
        }
        
        public void Init(VisualSmoothDelete smoothDelete, BaseDraggable baseDraggable)
        {
            this.baseDraggable = baseDraggable;
            this.visualSmoothDelete = smoothDelete;
            this.compositeSmoothDeleteExtension = this.visualSmoothDelete.GetComponentInChildren<CompositeSmoothDeleteExtension>();
        }
    }
   
}