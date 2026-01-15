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
    [SerializeField] private Transform boxInteractingObjectParent;
    [SerializeField] private Transform rootSequenceParent;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private string interactableTag;
    private SpriteRenderer[] staticObjectRenderers;
    private List<GameObject> interactableObjects;
    private SpineAnimMechanic[] spineAnimMechanics;
    private InteractingBox[] interactingBoxes;
    public SpriteRenderer[] StaticObjectRenderers => this.staticObjectRenderers;
    public SkeletonAnimation SkeletonAnimation => this.skeletonAnimation;
    public List<GameObject> InteractableObjects => this.interactableObjects;
    public SpineAnimMechanic[] SpineAnimMechanics => this.spineAnimMechanics;

    public InteractingBox[] InteractingBoxes => this.interactingBoxes;

    public void PrepareData()
    {
        this.interactableObjects = new List<GameObject>();
        this.staticObjectRenderers = this.staticObjectParent.GetComponentsInChildren<SpriteRenderer>();
        this.interactingBoxes = this.boxInteractingObjectParent.GetComponentsInChildren<InteractingBox>();
        this.spineAnimMechanics = this.rootSequenceParent.GetComponentsInChildren<SpineAnimMechanic>();

        foreach (Transform child in this.interactableObjectParent)
        {
            if (!child.tag.Equals(this.interactableTag))
            {
                continue;
            }

            this.interactableObjects.Add(child.gameObject);
        }

        foreach (Transform child in this.boxInteractingObjectParent)
        {
            if (!child.tag.Equals(this.interactableTag))
            {
                continue;
            }

            this.interactableObjects.Add(child.gameObject);
        }
    }
#endif
}