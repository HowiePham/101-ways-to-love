using Mimi.Audio;
using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Interactions.Draggable.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Generate
{
    public class DragSwitchObjectGenerator : MonoBehaviour
    {
        [SerializeField] private BaseDraggable tool;
        [SerializeField] private GameObject objectSwitch;
        [SerializeField] private BoxArea boxArea;
        [SerializeField] private bool isCheckReleaseFinger;
        [SoundKey]
        [SerializeField] private string soundKey;
        [SerializeField] private GameObject fixedActions;
        [SerializeField] private GameObject fetchedActions;
        
        #if UNITY_EDITOR
        [Button]
        public void Generate()
        {
            var sequence = new SequenceGenerator().Generate();
            var insideCondition = new InsideArea2DConditionGenerator(boxArea, tool.transform);
            var extension = new CompositeDragExtensionGenerator()
                .AddGenerator(new SwitchObjectDragExtensionGenerator(objectSwitch));

            var generate = new VisualDragGenerator((CompositeDragExtensionGenerator)extension, insideCondition, !isCheckReleaseFinger).Generate();
            generate.transform.SetParent(sequence.transform);
            new PlayAudioGenerator(soundKey).Generate().transform.SetParent(sequence.transform);
            
            if (fixedActions != null)
            {
                var fixedActionAfter = GameObject.Instantiate(fixedActions);
                fixedActionAfter.transform.SetParent(sequence.transform);
            }

            if (fetchedActions != null)
            {
                var fetchedActionAfter = GameObject.Instantiate(fetchedActions);
                fetchedActionAfter.transform.SetParent(sequence.transform);
                fetchedActionAfter.FetchDependency(objectSwitch.gameObject);
            }
            sequence.gameObject.name = gameObject.name;
        }
        
        [MenuItem("GameObject/Visual Actions/Mechanics/Dragging/Drag Switch Object", false, -10000)]
        public static void CreateDraggableStatic(MenuCommand menuCommand)
        {
            var generateInstance = new GameObject("DragSwitchObjectGenerator");
            var boxArea = new BoxArea2DGenerator().Generate();
            boxArea.transform.SetParent(generateInstance.transform);
            
            var tool = new DraggableGenerator().Generate();
            var returnPos = new GameObject("ReturnPos");
            returnPos.transform.position = tool.transform.position;
            tool.GetExtension<ReturnOnDeselect>().SetField("startPosition", returnPos.transform, AccessModifier.Private);
            tool.transform.SetParent(generateInstance.transform);
            returnPos.transform.SetParent(generateInstance.transform);
            
            var dragComponent = generateInstance.AddComponent<DragSwitchObjectGenerator>();
            
            dragComponent.SetField("tool", tool, AccessModifier.Private);
            dragComponent.SetField("boxArea", boxArea, AccessModifier.Private);
        }
        
        #endif
    }
}