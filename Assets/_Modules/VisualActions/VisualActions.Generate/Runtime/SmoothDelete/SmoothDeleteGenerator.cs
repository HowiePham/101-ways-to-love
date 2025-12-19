using System.Collections.Generic;
using Mimi.CommandEditor;
using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Generate;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Generate.Editor.GameObjects;
using Mimi.VisualActions.Interactions.Draggable.Extensions;
using Mimi.ScratchCardAsset;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace  Mimi.VisualActions.Generate
{
    public class SmoothDeleteGenerator : MonoBehaviour
    {
        [SerializeField] private BaseDraggable toolDelete;
        [SerializeField] private SpriteRenderer[] targetRenderers;
        [SerializeField] private ScratchCard.ScratchMode deleteMode;
        
#if UNITY_EDITOR
        [Button]
        public void Generate()
        {
            var visualParallel = new ParallelGenerator();

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                var sequence = new SequenceGenerator();
                var extension = new CompositeSmoothDeleteExtensionGenerator();
                if (toolDelete != null)
                {
                    extension.AddGenerator(new UsingToolSmoothDeleteExtensionGenerator(toolDelete));
                }
                var visualSmoothDelete = new VisualSmoothDeleteGenerator(extension , targetRenderers[i], deleteMode);
                if (deleteMode == ScratchCard.ScratchMode.Restore)
                {
                    sequence.AddGenerator(new SetActiveMultipleGameObjectsGenerator(new List<GameObject>()
                    {
                        targetRenderers[i].gameObject,
                    }, false));
                    sequence.AddGenerator(visualSmoothDelete);
                    sequence.AddGenerator(new SetActiveMultipleGameObjectsGenerator(new List<GameObject>()
                    {
                        targetRenderers[i].gameObject,
                    }, true));
                }
                else
                {
                    sequence.AddGenerator(visualSmoothDelete);
                }

                visualParallel.AddGenerator(sequence);
            }
            var objectSpawm = visualParallel.Generate();
            objectSpawm.name = gameObject.name;
        }
        
        
        [MenuItem("GameObject/Visual Actions/Mechanics/Deleting/Smooth Delete Generator", false, -10000)]
        public static void CreateDraggableStatic(MenuCommand menuCommand)
        {
            var generateInstance = new GameObject("SmoothDeleteGenerator");
            
            var tool = new DraggableGenerator().Generate();
            var returnPos = new GameObject("ReturnPos");
            returnPos.transform.position = tool.transform.position;
            tool.GetExtension<ReturnOnDeselect>().SetField("startPosition", returnPos.transform, AccessModifier.Private);
            tool.transform.SetParent(generateInstance.transform);
            returnPos.transform.SetParent(generateInstance.transform);
            
            var dragComponent = generateInstance.AddComponent<SmoothDeleteGenerator>();
            
            dragComponent.SetField("toolDelete", tool, AccessModifier.Private);
        }
        
#endif
    }
}