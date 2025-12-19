using System;
using System.Collections.Generic;
using System.Linq;
using ActorServices.Graphic.Draggable.Runtime;
using Mimi.Actor.Graphic.Core;
using Mimi.Audio;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Generate.Editor.GameObjects;
using Mimi.VisualActions.Generate.Editor.Graphic;
using Mimi.VisualActions.Interactions.Draggable.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Generate
{
    [Serializable]
    public class DragItemData
    {
        public DraggableItem itemDrag;
        public GameObject switchObject;
    }
    public class DragItemMultipleSwitchObjectsGenerator : MonoBehaviour
    {
        [SerializeField] private DragItemData[] draggableItemsData;
        [SerializeField] private BaseDraggable draggable;
        [SerializeField] private bool isReleaseToComplete;
        [SerializeField] private Vector2 boxSize;
        [SerializeField] private DragType dragType;
        [SoundKey]
        [SerializeField] private string soundKey;
        
        [SerializeField] private GameObject fixedActions;
        [SerializeField] private GameObject fetchedActions;
        public enum DragType
        {
            MultipleArea,
            MultipleAreaCanReplaced
        }
        #if UNITY_EDITOR
        [Button]
        public void Generate()
        {
           var visualParallel = new ParallelGenerator().Generate();

           var outputGraphic = draggable.GetComponentInChildren<MonoDraggableItemGraphic>();
           var draggableExtension = draggable.GetComponentInChildren<DraggableItemExtension>();
           draggableExtension.SetField("itemsTransform", draggableItemsData.Select(x => x.itemDrag).ToList(), AccessModifier.Private);
           EditorUtility.SetDirty(draggableExtension);
            foreach (var data in draggableItemsData)
            {
                var sequenceGenerator = new SequenceGenerator();
                
                var extension = new CompositeDragExtensionGenerator()
                    .AddGenerator(new DragItemExtensionGenerator(draggableExtension));
                var conditionGenerate = new CompleteAnyConditionGenerator();
                var area = new BoxArea2DGenerator().Generate();
                (area as BoxArea).SetSizeEditor(boxSize);
                area.transform.position = data.switchObject.transform.position;
                

                if (this.dragType == DragType.MultipleAreaCanReplaced)
                {
                    for (int i = 0; i < draggableItemsData.Length; i++)
                    {
                   
                        var insideCondition =
                            new InsideArea2DConditionGenerator(area, draggableItemsData[i].itemDrag.transform);
                        conditionGenerate.AddGenerator(insideCondition);
                    }
                }
                else
                {
                    conditionGenerate.AddGenerator(new InsideArea2DConditionGenerator(area, data.itemDrag.transform));
                }
               
                var visualDrag =new VisualDragGenerator((CompositeDragExtensionGenerator)extension, conditionGenerate, !isReleaseToComplete);
                
                sequenceGenerator.AddGenerator(visualDrag);

                if (dragType == DragType.MultipleAreaCanReplaced)
                {
                    sequenceGenerator.AddGenerator(new ShowGraphicGenerator(outputGraphic, false));
                   
                }
                else if (dragType == DragType.MultipleArea)
                {
                    sequenceGenerator.AddGenerator(
                        new SetActiveMultipleGameObjectsGenerator(new List<GameObject>() { data.itemDrag.gameObject }, false));
                }
              
                sequenceGenerator.AddGenerator(
                    new SetActiveMultipleGameObjectsGenerator(new List<GameObject>() { data.switchObject }, true));
                sequenceGenerator.AddGenerator(new PlayAudioGenerator(soundKey));
                var sequence = sequenceGenerator.Generate();
                if (fixedActions != null)
                {
                    var fixedActionAfter = GameObject.Instantiate(fixedActions);
                    fixedActionAfter.transform.SetParent(sequence.transform);
                }

                if (fetchedActions != null)
                {
                    var fetchedActionAfter = GameObject.Instantiate(fetchedActions);
                    fetchedActionAfter.transform.SetParent(sequence.transform);
                    fetchedActionAfter.FetchDependency(data.switchObject.gameObject);
                }
                sequence.transform.SetParent(visualParallel.transform);
                area.transform.SetParent(sequence.transform);
            }
            
            visualParallel.gameObject.name = this.gameObject.name;
        }
        
        [MenuItem("GameObject/Visual Actions/Mechanics/Dragging/DragSwitchItems", false, -10000)]
        public static void CreateDraggableStatic(MenuCommand menuCommand)
        {
            var generateInstance = new GameObject("DragItemsSwitchMultipleObjectGenerator");
            
            var boxArea = new BoxArea2DGenerator().Generate();
            boxArea.transform.SetParent(generateInstance.transform);
            
            var tool = new DraggableItemGenerator().Generate();
            var returnPos = new GameObject("ReturnPos");
            returnPos.transform.position = tool.transform.position;
            tool.GetExtension<ReturnOnDeselect>().SetField("startPosition", returnPos.transform, AccessModifier.Private);
            tool.transform.SetParent(generateInstance.transform);
            returnPos.transform.SetParent(generateInstance.transform);

            var dragItem = new ItemDraggableGenerator().Generate();
            dragItem.transform.SetParent(generateInstance.transform);
            
            var dragComponent = generateInstance.AddComponent<DragItemMultipleSwitchObjectsGenerator>();
            dragComponent.SetField("draggable", tool, AccessModifier.Private);
        }
        
#endif
    }
}