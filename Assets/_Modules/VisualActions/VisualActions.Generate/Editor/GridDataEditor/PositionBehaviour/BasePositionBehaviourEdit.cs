using System.Collections.Generic;
using System.Linq;
using Mimi.Interactions;
using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Generate.Editor;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Mimi.VisualActions.Generate.Editor
{
    using UnityEngine;

    [System.Serializable]
    public abstract class BasePositionBehaviourEdit<T> : IPositionBehaviourEdit<T>
        where T : BasePositionProcessor
    {
        [SerializeField,HideInInspector] private bool isToggle;
        //[InlineEditor(Expanded = true)]
        [SerializeReference]
        [OdinSerialize]
        private T processor;
        
        [SerializeField, HideInInspector] private CompositePositionProcessor compositePositionProcessor;
        [SerializeField, HideInInspector] private BaseDraggable draggable;
        
        public T Processor => processor;
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

        public virtual void TurnOnHandle()
        {
            if (this.compositePositionProcessor != null)
            {
                if (this.processor == null)
                {
                    this.processor = this.compositePositionProcessor.gameObject.AddComponent<T>();
                }
                var field = this.compositePositionProcessor
                    .GetFieldValue<BasePositionProcessor[]>("processors", AccessModifier.Private).ToList();
                field.Add(processor);
                this.compositePositionProcessor.SetField("processors", field.ToArray(), AccessModifier.Private);
            }
            EditorUtility.SetDirty(this.compositePositionProcessor);
        }

        public virtual void TurnOffHandle()
        {
            if (this.compositePositionProcessor != null && this.processor != null)
            {
                var field = this.compositePositionProcessor
                    .GetFieldValue<BasePositionProcessor[]>("processors", AccessModifier.Private).ToList();
                field.Remove(processor);
                this.compositePositionProcessor.SetField("processors", field.ToArray(), AccessModifier.Private);
            }
            EditorUtility.SetDirty(this.compositePositionProcessor);
        }

        public void SetCompositePositionProcessor(CompositePositionProcessor compositePositionProcessor)
        {
            this.compositePositionProcessor = compositePositionProcessor;
        }
       

        public void SetDraggable(BaseDraggable draggable)
        {
           this.draggable = draggable;
           this.compositePositionProcessor = this.draggable.GetComponentInChildren<CompositePositionProcessor>();
           BasePositionProcessor[] allProcessor = this.compositePositionProcessor.GetComponents<BasePositionProcessor>();
          
           var activeProsessor =  this.compositePositionProcessor
               .GetFieldValue<BasePositionProcessor[]>("processors", AccessModifier.Private).ToList();
           bool isChangeValue = false;
           foreach (var processor in allProcessor)
           {
               if (processor is T newProcessor)
               {
                   this.processor = newProcessor;
                   isChangeValue = true;
                   if (activeProsessor.Contains(processor))
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
               this.processor = null;
               isToggle = false;
           }
        }
    }
}