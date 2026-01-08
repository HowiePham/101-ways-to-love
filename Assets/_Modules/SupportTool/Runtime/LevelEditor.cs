using System.Collections.Generic;
using Mimi.Interactions.Dragging;
using Mimi.VisualActions.Spines;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Transform staticObjectParent;
    [SerializeField] private Transform interactableObjectParent;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private string interactableTag;
    private SpriteRenderer[] staticObjectRenderers;
    private List<SpriteRenderer> interactableObjectRenderers = new List<SpriteRenderer>();
    private List<GameObject> interactableObjects;
    private SpineAnimMechanic[] spineAnimMechanics;
    public SpriteRenderer[] StaticObjectRenderers => this.staticObjectRenderers;
    public List<SpriteRenderer> InteractableObjectRenderers => this.interactableObjectRenderers;
    public SkeletonAnimation SkeletonAnimation => this.skeletonAnimation;
    public List<GameObject> InteractableObjects => this.interactableObjects;
    public SpineAnimMechanic[] SpineAnimMechanics => this.spineAnimMechanics;

    public void PrepareData()
    {
        this.interactableObjectRenderers = new List<SpriteRenderer>();
        this.interactableObjects = new List<GameObject>();
        this.staticObjectRenderers = this.staticObjectParent.GetComponentsInChildren<SpriteRenderer>();
        this.spineAnimMechanics = this.gameObject.GetComponentsInChildren<SpineAnimMechanic>();

        foreach (Transform child in this.interactableObjectParent)
        {
            if (!child.tag.Equals(this.interactableTag))
            {
                continue;
            }

            this.interactableObjects.Add(child.gameObject);
        }

        foreach (GameObject draggable in this.InteractableObjects)
        {
            var draggableSpriteRenderer = draggable.GetComponentInChildren<SpriteRenderer>();
            this.InteractableObjectRenderers.Add(draggableSpriteRenderer);
        }
    }
#endif
}