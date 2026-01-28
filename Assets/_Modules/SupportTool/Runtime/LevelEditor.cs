using System;
using System.Collections.Generic;
using GameScenes;
using Mimi.VisualActions.Audio;
using Mimi.VisualActions.Spines;
using Spine.Unity;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    [SerializeField] private Transform staticObjectParent;
    [SerializeField] private Transform interactableObjectParent;
    [SerializeField] private Transform boxInteractingObjectParent;
    [SerializeField] private Transform rootSequenceParent;
    [SerializeField] private Transform hintParent;
    [SerializeField] private Transform generalLevelSound;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private string interactableTag;
    private List<GameObject> staticObjects;
    private List<GameObject> interactableObjects;
    private SpineAnimMechanic[] spineAnimMechanics;
    private InteractingBox[] interactingBoxes;
    private PlayAudio[] levelSounds;
    public List<GameObject> StaticObjects => this.staticObjects;
    public SkeletonAnimation SkeletonAnimation => this.skeletonAnimation;
    public List<GameObject> InteractableObjects => this.interactableObjects;
    public SpineAnimMechanic[] SpineAnimMechanics => this.spineAnimMechanics;
    public InteractingBox[] InteractingBoxes => this.interactingBoxes;
    public Transform RootSequenceParent => this.rootSequenceParent;
    public Transform HintParent => this.hintParent;
    public PlayAudio[] LevelSounds => this.levelSounds;

    public void PrepareData()
    {
        this.interactableObjects = new List<GameObject>();
        this.staticObjects = new List<GameObject>();
        this.interactingBoxes = this.boxInteractingObjectParent.GetComponentsInChildren<InteractingBox>();
        this.spineAnimMechanics = this.RootSequenceParent.GetComponentsInChildren<SpineAnimMechanic>();
        this.levelSounds = this.generalLevelSound.GetComponentsInChildren<PlayAudio>();

        foreach (Transform child in this.interactableObjectParent)
        {
            if (!child.tag.Equals(this.interactableTag))
            {
                continue;
            }

            this.interactableObjects.Add(child.gameObject);
        }

        foreach (Transform child in this.staticObjectParent)
        {
            this.staticObjects.Add(child.gameObject);
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
}