using Mimi.Reflection.Extensions;
using Mimi.VisualActions.ControlFlow;
using Spine.Unity;
using UnityEngine;
using VisualActions.Areas;
using VisualActions.VisualActions.GameObjects.Runtime;

public class MixingMechanicGenerator : MechanicGenerator
{
    private string mixMechanicBlueprintAddress = $"{MechanicBlueprintAddress}/MixMechanic/";

    private TapMechanicGenerator tapMechanicGenerator;
    private DragMechanicGenerator dragMechanicGenerator;

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation, GameObject interactableObjectParent, GameObject boxInteractionParent,Sprite sprite)
    {
        if (menuName.Contains("Tap") && menuName.Contains("Drag"))
        {
            this.tapMechanicGenerator = new TapMechanicGenerator();
            this.dragMechanicGenerator = new DragMechanicGenerator();

            CreateTapDragMechanic(menuName, objectName, skeletonAnimation, interactableObjectParent, boxInteractionParent,sprite);
        }
    }

    private void CreateTapDragMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation,
        GameObject interactableObjectParent, GameObject boxInteractionParent,Sprite sprite)
    {
        var mechanicParent = new GameObject($"{menuName}_{objectName}");
        mechanicParent.AddComponent<VisualSequence>();
        
        BoxArea tapArea = this.tapMechanicGenerator.CreateTapArea(objectName, boxInteractionParent);
        GameObject tapBlueprint = this.tapMechanicGenerator.CreateMechanic("Tap_Active", skeletonAnimation, tapArea, "TapArea");

        GameObject draggableObject = this.dragMechanicGenerator.CreateDraggableObject(objectName, interactableObjectParent,sprite);
        GameObject dragBlueprint = this.dragMechanicGenerator.CreateDragSingleResult("Drag_True", draggableObject, skeletonAnimation, boxInteractionParent);
        
        GameObject actionObject = CreateDisableTargetMechanic(draggableObject);
        actionObject.transform.SetParent(mechanicParent.transform);
        tapBlueprint.transform.SetParent(mechanicParent.transform);
        dragBlueprint.transform.SetParent(mechanicParent.transform);
        HandleEnableTarget(tapBlueprint, draggableObject);
    }

    private void HandleEnableTarget(GameObject mechanicBlueprint, GameObject targetObject)
    {
        SetActiveMultipleGameObjectsAction[] activeMultipleFieldGameObjects = mechanicBlueprint.GetComponentsInChildren<SetActiveMultipleGameObjectsAction>();
        foreach (SetActiveMultipleGameObjectsAction action in activeMultipleFieldGameObjects)
        {
            string gameObjectName = action.gameObject.name;
            if (!gameObjectName.Contains("On"))
            {
                continue;
            }

            GameObject[] oldActiveGameObjects = action.GameObjects;
            var newActiveGameObjects = new GameObject[oldActiveGameObjects.Length + 1];
            oldActiveGameObjects.CopyTo(newActiveGameObjects, 0);
            newActiveGameObjects[newActiveGameObjects.Length - 1] = targetObject;
            action.SetField("gameObjects", newActiveGameObjects, AccessModifier.Private);
        }
    }

    private GameObject CreateDisableTargetMechanic(GameObject targetObject)
    {
        var actionObject = new GameObject("SetActive_Off");
        var disableObjectAction = actionObject.AddComponent<SetActiveMultipleGameObjectsAction>();
        disableObjectAction.SetField("gameObjects", new GameObject[] { targetObject }, AccessModifier.Private);
        disableObjectAction.SetField("status", false, AccessModifier.Private);

        return actionObject;
    }
}