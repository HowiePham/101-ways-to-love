using System;
using System.Collections.Generic;
using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Interactions.Draggable.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Generate
{
    [Serializable]
    public class ObjectSwitchData
    {
        public BaseDraggable tool;
        public GameObject objectSwitch;
        public BoxArea boxArea;
    }
    public class DragSwitchMultipleObjectsGenerator : MonoBehaviour
    {
        [SerializeField] private List<ObjectSwitchData> objectSwitchDatas;
        [SerializeField] private bool isCheckReleaseFinger;
        
#if UNITY_EDITOR
        [Button]
        public void Generate()
        {
            var visualParallel = new ParallelGenerator();

            foreach (var data in objectSwitchDatas)
            {
                var extension = new SwitchObjectDragExtensionGenerator(data.objectSwitch);
                var conditionGenerate = new CompleteAnyConditionGenerator();
                for (int i = 0; i < objectSwitchDatas.Count; i++)
                {
                    var insideCondition =
                        new InsideArea2DConditionGenerator(data.boxArea, objectSwitchDatas[i].tool.transform);
                    conditionGenerate.AddGenerator(insideCondition);
                }
                var generate =new VisualDragGenerator(extension, conditionGenerate, !isCheckReleaseFinger);
                visualParallel.AddGenerator(generate);
            }
            var result = visualParallel.Generate();
            result.gameObject.name = this.gameObject.name;
        }
        
        [MenuItem("GameObject/Visual Actions/Mechanics/Dragging/Drag Switch Multiple Object", false, -10000)]
        public static void CreateDraggableStatic(MenuCommand menuCommand)
        {
            var generateInstance = new GameObject("DragSwitchMultipleObjectGenerator");
            
            var boxArea = new BoxArea2DGenerator().Generate();
            boxArea.transform.SetParent(generateInstance.transform);
            
            var tool = new DraggableGenerator().Generate();
            var returnPos = new GameObject("ReturnPos");
            returnPos.transform.position = tool.transform.position;
            tool.GetExtension<ReturnOnDeselect>().SetField("startPosition", returnPos.transform, AccessModifier.Private);
            tool.transform.SetParent(generateInstance.transform);
            returnPos.transform.SetParent(generateInstance.transform);
            
            var dragComponent = generateInstance.AddComponent<DragSwitchMultipleObjectsGenerator>();
        }
        
#endif
    }
}