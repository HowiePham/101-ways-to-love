using System.Collections.Generic;
using Mimi.Audio;
using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Generate.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Generate
{
    public class DragToolToMultipleObjectsGenerator : MonoBehaviour
    {
        [SerializeField] private List<GameObject> objectsRenderer;
        [SerializeField] private Transform checkTransform;
        [SerializeField] private GameObject fixedAction;
        [SerializeField] private GameObject fetchedAction;
        [SerializeField] private Vector2 boxSize = Vector2.one;
        [SoundKey]
        [SerializeField] private string soundKey;
        #if UNITY_EDITOR

        [Button]
        public void Generate()
        {
            var visualParallel = new ParallelGenerator().Generate();
            foreach (var item in objectsRenderer)
            {
                var sequence = new SequenceGenerator().Generate();
                sequence.transform.SetParent(visualParallel.transform);
                var boxArea = new BoxArea2DGenerator().Generate();
                
                var visualDragGenerator = new VisualDragGenerator(new CompositeDragExtensionGenerator(),
                    new InsideArea2DConditionGenerator(boxArea, checkTransform), true).Generate();
                visualDragGenerator.transform.SetParent(sequence.transform);
                boxArea.transform.SetParent(visualDragGenerator.transform);
                (boxArea as BoxArea).SetSizeEditor(boxSize);
                boxArea.transform.position = item.gameObject.transform.position;

                var sound = new PlayAudioGenerator(soundKey).Generate();
                sound.transform.SetParent(sequence.transform);
                if (fixedAction != null)
                {
                    var fixedActionAfter = GameObject.Instantiate(fixedAction);
                    fixedActionAfter.transform.SetParent(sequence.transform);
                }

                if (fetchedAction != null)
                {
                    var fetchedActionAfter = GameObject.Instantiate(fetchedAction);
                    fetchedActionAfter.transform.SetParent(sequence.transform);
                    fetchedActionAfter.FetchDependency(item);
                }
             
                
                sequence.transform.SetParent(visualParallel.transform);
            }

            visualParallel.gameObject.name = gameObject.name;
            EditorGUIUtility.PingObject(visualParallel);
        }
        #endif
    }
}