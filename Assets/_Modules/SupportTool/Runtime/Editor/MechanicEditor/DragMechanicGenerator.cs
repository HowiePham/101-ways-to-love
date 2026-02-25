using Mimi.Reflection.Extensions;
using Mimi.VisualActions.ControlFlow;
using Mimi.VisualActions.Dragging;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

public class DragMechanicGenerator : MechanicGenerator
{
    private string dragBlueprintAddress = $"{MechanicBlueprintAddress}/Drag/";
    private string draggableObjectBlueprintAddress = $"{MechanicBlueprintAddress}/Drag/Draggable_Object.prefab";
    private const string DragTrueName = "Drag_True";
    private const string DragFalseName = "Drag_False";

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation, 
        GameObject interactableObjectParent, GameObject boxInteractionParent, Sprite sprite)
    {
        GameObject draggableObject;
        if (menuName.Contains("2Result"))
        {
            draggableObject = CreateDraggableObject(objectName, interactableObjectParent, sprite);
            CreateDragDuoResult(menuName, draggableObject, skeletonAnimation, boxInteractionParent);

            return;
        }

        draggableObject = CreateDraggableObject(objectName, interactableObjectParent, sprite);
        SetParent(draggableObject.transform, interactableObjectParent.transform);
        CreateDragSingleResult(menuName, draggableObject, skeletonAnimation, boxInteractionParent);
    }

    private void CreateDragDuoResult(string menuName, GameObject draggableObject, SkeletonAnimation skeletonAnimation, GameObject boxInteractionParent)
    {
        GameObject trueBlueprintObject = CreateMechanicBlueprint(this.dragBlueprintAddress, DragTrueName);
        GameObject falseBlueprintObject = CreateMechanicBlueprint(this.dragBlueprintAddress, DragFalseName);

        if (trueBlueprintObject == null || falseBlueprintObject == null)
        {
            return;
        }

        HandleDragMechanicBlueprint(trueBlueprintObject, draggableObject, skeletonAnimation, "Draggable", boxInteractionParent);
        HandleDragMechanicBlueprint(falseBlueprintObject, draggableObject, skeletonAnimation, "Draggable", boxInteractionParent);

        var dragDuoFlowParent = new GameObject();
        dragDuoFlowParent.name = $"{menuName}";
        dragDuoFlowParent.AddComponent<VisualParallel>();
        trueBlueprintObject.transform.SetParent(dragDuoFlowParent.transform);
        falseBlueprintObject.transform.SetParent(dragDuoFlowParent.transform);

        AddAutoRenameComponent(dragDuoFlowParent, draggableObject, dragDuoFlowParent.name, "Draggable");
    }

    public GameObject CreateDragSingleResult(string menuName, GameObject draggableObject, SkeletonAnimation skeletonAnimation, GameObject boxInteractionParent)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(this.dragBlueprintAddress, menuName);

        if (blueprintObject == null)
        {
            return null;
        }

        HandleDragMechanicBlueprint(blueprintObject, draggableObject, skeletonAnimation, "Draggable", boxInteractionParent);
        return blueprintObject;
    }

    public void HandleDragMechanicBlueprint(GameObject blueprintObject, GameObject draggableObject,
        SkeletonAnimation skeletonAnimation, string suffix, GameObject boxInteractionParent)
    {
        BoxArea boxArea = CreateBoxArea();
        boxArea.name = $"BoxDestination_{blueprintObject.name}";
        SetParent(boxArea.transform, boxInteractionParent.transform);

        var insideArea2D = blueprintObject.GetComponentInChildren<InsideArea2D>();
        insideArea2D.SetField("checkTransform", draggableObject.transform, AccessModifier.Private);
        insideArea2D.SetField("targetArea", boxArea, AccessModifier.Private);

        var disableWhileRunningAnimParent = GameObject.Find("DisableWhileRunningAnimation");
        var gameObjects = new GameObject[] { draggableObject, boxInteractionParent, disableWhileRunningAnimParent };
        HandleSetActiveCommandInMechanic(blueprintObject, gameObjects);
        HandleAnimInMechanic(blueprintObject, draggableObject, skeletonAnimation, suffix);

        AddAutoRenameComponent(blueprintObject, draggableObject, $"{blueprintObject.name}", suffix);
        AddAutoRenameComponent(boxArea.gameObject, draggableObject, $"{boxArea.name}", suffix);
        AddAutoRenameComponent(insideArea2D.gameObject, draggableObject, $"{insideArea2D.name}", suffix);
    }

    public GameObject CreateDraggableObject(string objectName, GameObject interactableObjectParent, Sprite sprite)
    {
        var draggableObjectTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{this.draggableObjectBlueprintAddress}");
        var draggableObject = (GameObject)PrefabUtility.InstantiatePrefab(draggableObjectTemplate);
        draggableObject.name = $"Draggable_{objectName}";
        SetParent(draggableObject.transform, interactableObjectParent.transform);
        var objectRenderer = draggableObject.GetComponentInChildren<SpriteRenderer>();

        if (objectRenderer != null && sprite != null)
        {
            objectRenderer.sprite = sprite;
        }

        return draggableObject;
    }
}