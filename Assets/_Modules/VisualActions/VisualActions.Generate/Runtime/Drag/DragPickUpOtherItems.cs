using System.Collections.Generic;
using ActorServices.Graphic.Draggable.Runtime;
using Mimi.Audio;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Generate.Editor.GameObjects;
using Mimi.VisualActions.Generate.Editor.Graphic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Generate
{
    public class DragPickUpOtherItems : MonoBehaviour
    {
        [SerializeField] private ConditionCompleteType conditionCompleteType;
        [SerializeField, ShowIf("IsOutSide")] private BoxArea boxAreaOutside;
        [SerializeField] private bool isUsingSpriteRenderer = false;
        [SerializeField, ShowIf("isUsingSpriteRenderer")] private List<SpriteRenderer> spriteRenderers;
        [SerializeField, ShowIf("@!isUsingSpriteRenderer")] private List<DraggableItem> draggableItems;
        [SerializeField] private BaseDraggable draggable;
        [SerializeField] private bool isReleaseToComplete;
        [SerializeField,SoundKey] private string soundKey;
        [SerializeField] private GameObject fixedActions;
        [SerializeField] private GameObject fetchedActions;

        public enum ConditionCompleteType
        {
            IsOutSide,
            IsPickUp
        }
        bool IsOutSide => conditionCompleteType == ConditionCompleteType.IsOutSide;
        #if UNITY_EDITOR

        [Button]
        public void Generate()
        {
            if (isUsingSpriteRenderer)
            {
                FetchDraggableItems();
            }
            var visualParallel = new ParallelGenerator().Generate();
            
            var draggableExtension = draggable.GetComponentInChildren<DraggableItemExtension>();
            draggableExtension.SetField("itemsTransform", draggableItems, AccessModifier.Private);
            EditorUtility.SetDirty(draggableExtension);
            foreach (var item in draggableItems)
            {
                var sequenceGenerator = new SequenceGenerator();
                
                var extension = new CompositeDragExtensionGenerator()
                    .AddGenerator(new DragItemExtensionGenerator(draggableExtension));
                IVisualConditionGenerator conditionGenerator = null;
                if (IsOutSide)
                {
                    conditionGenerator =
                        new InvertConditionGenerator(
                            new InsideArea2DConditionGenerator(boxAreaOutside, item.transform));
                    
                }
                else
                {
                    conditionGenerator = new IsItemPickUpConditionGenerator(item);

                }
                
              
                var visualDrag =new VisualDragGenerator((CompositeDragExtensionGenerator)extension, conditionGenerator, !isReleaseToComplete);
                
                sequenceGenerator.AddGenerator(visualDrag);
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
                    fetchedActionAfter.FetchDependency(item.gameObject);
                }
                sequence.transform.SetParent(visualParallel.transform);
            }
            
            visualParallel.gameObject.name = this.gameObject.name;
            EditorGUIUtility.PingObject(visualParallel);
        }

        private void FetchDraggableItems()
        {
            this.draggableItems = new List<DraggableItem>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                var draggableItemSpawn = new ItemDraggableGenerator().Generate();
                var dragSpriteRenderer = draggableItemSpawn.GetComponentInChildren<SpriteRenderer>();
                draggableItemSpawn.transform.position = spriteRenderer.transform.position;
                dragSpriteRenderer.sprite = spriteRenderer.sprite;
                dragSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                dragSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder;
                draggableItemSpawn.transform.SetParent(this.transform);
                this.draggableItems.Add(draggableItemSpawn);
            }
        }
        #endif
    }
}