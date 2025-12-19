using System.Collections.Generic;
using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Deleting;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Generate.Editor.GameObjects;
using Mimi.VisualActions.Generate.Editor.SmoothDeleteExtension;
using Mimi.ScratchCardAsset;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VisualActions.VisualActions.GameObjects.Runtime;

namespace Mimi.VisualActions.Generate
{
    public class VisualSmoothDeleteEditor : VisualMechanicEditor
    {
        [SerializeField, HideInInspector] private VisualToolEditor visualToolEditor;
        
        [SerializeField, ReadOnly] private VisualSmoothDelete visualSmoothDelete;
        [SerializeField, HideInInspector] private CompositeSmoothDeleteExtension compositeSmoothDelete;
        [SerializeField, HideInInspector] private SetActiveMultipleGameObjectsAction setActiveRenderer;
        [ShowInInspector] 
        [PropertyOrder(-10)]
        public ScratchCard.ScratchMode ScratchMode
        {
            get
            {
                if (visualSmoothDelete == null)
                {
                    return ScratchCard.ScratchMode.Erase;
                }
                return visualSmoothDelete.GetFieldValue<ScratchCard.ScratchMode>("scratchMode", AccessModifier.Private);
            }
            set
            {
                if (visualSmoothDelete != null)
                {
                    visualSmoothDelete.SetField("scratchMode", value, AccessModifier.Private);
                    setActiveRenderer.gameObject.SetActive(value == ScratchCard.ScratchMode.Restore);
                    EditorUtility.SetDirty(visualSmoothDelete);
                    EditorUtility.SetDirty(setActiveRenderer);
                }
            }
        }

        [ShowInInspector]
        [PropertyOrder(-9)]
        [Range(0,1f)]
        public float TargetDeletePercentage
        {
            get
            {
                if (visualSmoothDelete == null)
                {
                    return 0f;
                }
                return visualSmoothDelete.targetDeletePercentage;
            }
            set
            {
                if (visualSmoothDelete != null)
                {
                    visualSmoothDelete.targetDeletePercentage = value;
                    EditorUtility.SetDirty(visualSmoothDelete);
                }
            }
        }
        [ShowInInspector] 
        [PropertyOrder(-8)]
        public SpriteRenderer DeleteRenderer
        {
            get
            {
                if (visualSmoothDelete == null)
                {
                    return null;
                }
                return visualSmoothDelete.GetFieldValue<SpriteRenderer>("maskSpriteRenderer", AccessModifier.Private);
            }
            set
            {
                if (visualSmoothDelete != null)
                {
                    var spriteRenderer = visualSmoothDelete.GetFieldValue<SpriteRenderer>("maskSpriteRenderer", AccessModifier.Private);
                    spriteRenderer.sprite = value.sprite;
                    spriteRenderer.sortingOrder = value.sortingOrder;
                    spriteRenderer.sortingLayerName = value.sortingLayerName;
                    spriteRenderer.transform.position = value.transform.position;
                    var gameObjectList =
                        setActiveRenderer.GetFieldValue<GameObject[]>("gameObjects", AccessModifier.Private);
                    gameObjectList[0] = value.gameObject;
                    EditorUtility.SetDirty(spriteRenderer);
                    EditorUtility.SetDirty(setActiveRenderer);
                }
            }
        }

        public BaseDraggable Draggable => visualToolEditor.GetDraggable();
        [EditData(typeof(ISmoothDeleteEdit))]
        [SerializeReference] private List<IDataEdit> extensions = new List<IDataEdit>();
#if UNITY_EDITOR
        [Button]
        public void Refresh()
        {
            visualToolEditor = gameObject.GetComponent<VisualToolEditor>();
            this.visualSmoothDelete = gameObject.GetComponentInChildren<VisualSmoothDelete>();
            this.compositeSmoothDelete =
                this.visualSmoothDelete.GetComponentInChildren<CompositeSmoothDeleteExtension>();
            if (setActiveRenderer == null)
            {
                setActiveRenderer = new SetActiveMultipleGameObjectsGenerator(new List<GameObject>()
                {
                    this.DeleteRenderer.gameObject
                }, true).Generate() as SetActiveMultipleGameObjectsAction;
                setActiveRenderer.transform.SetParent(visualSmoothDelete.transform.parent);
                setActiveRenderer.transform.SetAsLastSibling();
            }

            foreach (var extension in extensions)
            {
                (extension as ISmoothDeleteEdit).Init(this.visualSmoothDelete, this.Draggable);
            }

            ScratchMode = ScratchCard.ScratchMode.Erase;
        }
        
        
#endif
    }
}