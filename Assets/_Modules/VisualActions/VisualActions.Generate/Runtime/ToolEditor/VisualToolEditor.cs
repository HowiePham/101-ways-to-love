using System;
using System.Collections.Generic;
using Mimi.Actor.Graphic.Core;
using Mimi.Actor.Graphic.Spine;
using Mimi.Actor.Graphic.SpriteRenderer;
using Mimi.Interactions;
using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Data;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Generate.Editor.DraggableExtension;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using VisualActions.Areas;

namespace  Mimi.VisualActions.Generate
{
    public class VisualToolEditor : MonoBehaviour
    {
        [PropertyOrder(-11)]
        [SerializeField]
        private BaseDraggable draggable;

        #region Collider

        [SerializeField, HideInInspector]
        private BoxCollider boxCollider;
        [ShowInInspector]
        [PropertyOrder(-10)]
        public Vector3 ColliderSize
        {
            get => boxCollider != null ? boxCollider.size : Vector3.zero;
            set
            {
                if (boxCollider != null)
                    boxCollider.size = value;
            }
        }

        #endregion

        #region Offset
        [SerializeField, HideInInspector] private Vector3Field offsetField;
        [ShowInInspector]
        [PropertyOrder(-10)]
        public Vector3 ToolOffset
        {
            get => offsetField != null ? offsetField.GetValue() : Vector3.zero;
            set
            {
                if (offsetField != null)
                {
                    offsetField.SetValue(value);
                    #if UNITY_EDITOR
                    EditorUtility.SetDirty(offsetField);
                    #endif
                }
            }
        }

        #endregion
       
        
        [SerializeField] 
        [OnAddItem(nameof(OnAddItemGraphic))]
        [OnRemoveItem(nameof(OnRemoveItemGraphic))]
        private List<GraphicEditData> graphicEditDatas = new List<GraphicEditData>();
        
        private MonoCompositeGraphic compositeGraphic;
        
       
        [EditData(typeof(IPositionBehaviourEdit))]
        [SerializeReference] private List<IDataEdit> movementBehaviours = new List<IDataEdit>();
        
        [EditData(typeof(IDraggableExtensionEdit))]
        [SerializeReference]
        private List<IDataEdit> toolExtensions = new List<IDataEdit>();

        public BaseDraggable GetDraggable()
        {

            return this.draggable;
        }
#if UNITY_EDITOR

        private void OnValidate()
        {
            Undo.ClearUndo(this);
        }

        [Button]
        public void Refresh()
        {
            SetDraggable(draggable);
        }
           public void SetDraggable(BaseDraggable newDraggable)
        {
            this.draggable = newDraggable;
            boxCollider = draggable.GetComponent<BoxCollider>();
            offsetField = draggable.GetComponent<Vector3Field>();
            foreach (var editData in movementBehaviours)
            {
                if (editData is IPositionBehaviourEdit positionBehaviourEdit)
                {
                    positionBehaviourEdit.SetDraggable(newDraggable);
                }
            }

            foreach (var editData in toolExtensions)
            {
                if (editData is IDraggableExtensionEdit draggableExtensionEdit)
                {
                    draggableExtensionEdit.SetDraggable(newDraggable);
                }
            }
            SetGraphic(newDraggable);
            EditorUtility.SetDirty(gameObject);
        }

        private void OnAddItemGraphic(GraphicEditData data)
        {
            data.OnAddItemGraphic(compositeGraphic);
        }

        private void OnRemoveItemGraphic(GraphicEditData data)
        {
            data.OnRemoveItemGraphic();
        }
        
        private void SetGraphic(BaseDraggable newDraggable)
        {
            this.draggable = newDraggable;
            this.compositeGraphic = newDraggable.GetComponentInChildren<MonoCompositeGraphic>();
            var graphicDatas = this.compositeGraphic.GetFieldValue<BaseMonoGraphic[]>("graphics", AccessModifier.Private);

            graphicEditDatas.Clear();
            var tempList = new List<GraphicEditData>();
            foreach (var graphicData in graphicDatas)
            {
                var newGraphicData = new GraphicEditData();
                if (graphicData is MonoSpriteRendererGraphic)
                {
                    newGraphicData.graphicType = GraphicType.SpriteRenderer;
                    newGraphicData.SpriteRenderer =
                        graphicData.GetFieldValue<SpriteRenderer>("_spriteRenderer", AccessModifier.Private);
                }
                else if (graphicData is MonoSpineGraphic)
                {
                    newGraphicData.graphicType = GraphicType.Spine;
                    newGraphicData.SkeletonAnimation =
                        graphicData.GetFieldValue<SkeletonAnimation>("skeletonAnimation", AccessModifier.Private);
                }
                newGraphicData.OnInit(compositeGraphic , graphicData);
                tempList.Add(newGraphicData);
            }

            graphicEditDatas = tempList;

           
        }
        #endif
    }
}