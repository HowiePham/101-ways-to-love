using System.Collections.Generic;
using Mimi.Interactions.Dragging;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Transform staticObjectParent;
    [SerializeField] private Transform interactableObjectParent;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    private SpriteRenderer[] staticObjectRenderers;
    private List<SpriteRenderer> interactableObjectRenderers = new List<SpriteRenderer>();
    private BaseDraggable[] baseDraggables;
    public SpriteRenderer[] StaticObjectRenderers => this.staticObjectRenderers;
    public List<SpriteRenderer> InteractableObjectRenderers => this.interactableObjectRenderers;
    public SkeletonAnimation SkeletonAnimation => this.skeletonAnimation;
    public BaseDraggable[] BaseDraggables => this.baseDraggables;

    public void PrepareData()
    {
        this.interactableObjectRenderers = new List<SpriteRenderer>();
        this.staticObjectRenderers = this.staticObjectParent.GetComponentsInChildren<SpriteRenderer>();
        this.baseDraggables = this.interactableObjectParent.GetComponentsInChildren<BaseDraggable>();

        foreach (BaseDraggable draggable in this.BaseDraggables)
        {
            var draggableSpriteRenderer = draggable.GetComponentInChildren<SpriteRenderer>();
            this.InteractableObjectRenderers.Add(draggableSpriteRenderer);
        }
    }
#endif
}