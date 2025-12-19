using System;
using System.Linq;
using Mimi.Actor.Graphic.Core;
using Mimi.Actor.Graphic.SpriteRenderer;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Generate.Editor.GraphicGenerator;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    public enum GraphicType
    {
        SpriteRenderer,
        Spine
    }
    [Serializable]
    public class GraphicEditData
    {   
        [OnValueChanged("OnGraphicTypeChanged")]
        public GraphicType graphicType;
        [InlineEditor,OnValueChanged("OnTransformChanged")]
        public Transform targetTransform;
        
        [ShowIf("@IsSpriteRenderer(GraphicType.SpriteRenderer)")]
        [ReadOnly]
        public SpriteRenderer SpriteRenderer;
        [ShowIf("@IsSpriteRenderer(GraphicType.SpriteRenderer)")]
        public SpriteRendererEditData SpriteRendererEditData;
        
        [ShowIf("@IsSpriteRenderer(GraphicType.Spine)")]
        [InlineEditor]
        public SkeletonAnimation SkeletonAnimation;
        
        private Transform defaultTransform;
        private MonoCompositeGraphic defaultCompositeGraphic;
        private BaseMonoGraphic targetGraphic;
        public void OnInit(MonoCompositeGraphic compositeGraphic, BaseMonoGraphic targetGraphic)
        {
            this.defaultCompositeGraphic = compositeGraphic;
            this.targetGraphic = targetGraphic;
            SetDefaultTransform(this.targetGraphic.transform);
            this.targetTransform = this.targetGraphic.transform;
            if (targetGraphic is MonoSpriteRendererGraphic)
            {
                this.SpriteRenderer = targetGraphic.GetComponent<SpriteRenderer>();
                this.SpriteRendererEditData = new SpriteRendererEditData();
                this.SpriteRendererEditData.InitData(this.SpriteRenderer);
            }
        }

        public void SetCompositeGraphic(MonoCompositeGraphic compositeGraphic)
        {
            defaultCompositeGraphic = compositeGraphic;
        }
        public void SetDefaultTransform(Transform defaultTransform)
        {
            this.defaultTransform = defaultTransform;
        }

        public void SetTargetGraphic(MonoCompositeGraphic graphic)
        {
            targetGraphic = graphic;
        }

        private bool IsSpriteRenderer(GraphicType checkType)
        {
            return graphicType == checkType;
        }

        public void OnTransformChanged(Transform newTransform)
        {
            this.targetTransform = defaultTransform;
        }

        public void OnAddItemGraphic(MonoCompositeGraphic graphicData)
        {
            this.defaultCompositeGraphic = graphicData;
            CreateNewGraphic(this.graphicType);
            EditorUtility.SetDirty(defaultCompositeGraphic);
        }

        public void OnRemoveItemGraphic()
        {
            DestroyCurrentGraphic();
        }

        private void DestroyCurrentGraphic()
        {
            var modifyList = this.defaultCompositeGraphic.GetGraphics().ToList();
            modifyList.Remove(this.targetGraphic);
            this.defaultCompositeGraphic.SetField("graphics",modifyList.ToArray(), AccessModifier.Private);
            if (targetTransform != null)
            {
                GameObject.DestroyImmediate(targetTransform.gameObject);
            }
            EditorUtility.SetDirty(defaultCompositeGraphic);
        }
        private void CreateNewGraphic(GraphicType graphicType)
        {
            
            Transform newTransform = null;
            if (graphicType == GraphicType.SpriteRenderer)
            {
                newTransform = new SpriteRendererGenerator().Generate().transform;
                this.SpriteRenderer = newTransform.GetComponent<SpriteRenderer>();
                this.SpriteRendererEditData = new SpriteRendererEditData();
                this.SpriteRendererEditData.InitData(this.SpriteRenderer);
            }
            else
            {
                newTransform = new SpineGraphicGenerator().Generate().transform;
                this.SkeletonAnimation = newTransform.GetComponent<SkeletonAnimation>();
            }
            var newGraphic = newTransform.GetComponent<BaseMonoGraphic>();
            this.targetGraphic = newGraphic;
            newTransform.SetParent(this.defaultCompositeGraphic.transform);
            newTransform.localPosition = Vector3.zero;
            
            var modifyGraphics = this.defaultCompositeGraphic.GetGraphics().ToList();
            modifyGraphics.Add(newGraphic);
            this.defaultCompositeGraphic.SetField("graphics",modifyGraphics.ToArray(), AccessModifier.Private);
            
            SetDefaultTransform(newTransform);
            this.targetTransform = newTransform;
        }
        public void OnGraphicTypeChanged(GraphicType checkType)
        {
            DestroyCurrentGraphic();
            CreateNewGraphic(checkType);
        }
    }
}