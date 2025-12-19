using System.Collections.Generic;
using System.Linq;
using Mimi.EffectMaker.Core;
using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Generate.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    public class GenerateDraggableItem : MonoBehaviour
    {
        [SerializeField] private List<SpriteRenderer> spriteRenderers;

       #if UNITY_EDITOR

        [Button]
        public void Generate()
        {
            foreach (var spriteRenderer in spriteRenderers)
            {
                var draggableItemSpawn = new ItemDraggableGenerator().Generate();
                var dragSpriteRenderer = draggableItemSpawn.GetComponentInChildren<SpriteRenderer>();
                draggableItemSpawn.transform.position = spriteRenderer.transform.position;
                dragSpriteRenderer.sprite = spriteRenderer.sprite;
                dragSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                dragSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder;
                draggableItemSpawn.transform.SetParent(this.transform);

                var compositeExtension = draggableItemSpawn.GetComponentInChildren<CompositeItemExtension>();
                var allComponents = gameObject.GetComponents<MonoItemDragExtension>();
                var componentsContainer = compositeExtension.GetFieldValue<MonoItemDragExtension[]>("compositeItems", AccessModifier.Private).ToList();

                foreach (var component in allComponents)
                {
                    var newComponent = compositeExtension.gameObject.AddComponent(component.GetType());
                    EditorUtility.CopySerialized(component, newComponent);
                    if (newComponent is ItemPlayEffectWhenPickUp effect)
                    {
                        var newEffect = GameObject.Instantiate(effect
                            .GetFieldValue<MonoEffectMaker>("effectMaker", AccessModifier.Private).gameObject, newComponent.transform);
                        newEffect.FetchDependency(draggableItemSpawn.gameObject);
                        newComponent.SetField("effectMaker", newEffect.GetComponent<MonoEffectMaker>(), AccessModifier.Private);
                    }
                    componentsContainer.Add(newComponent as MonoItemDragExtension);
                }
                compositeExtension.SetField("compositeItems", componentsContainer.ToArray(), AccessModifier.Private);
            }
        }
        #endif
    }
}