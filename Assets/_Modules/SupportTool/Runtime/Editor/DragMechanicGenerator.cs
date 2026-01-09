using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Dragging;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

public class DragMechanicGenerator : MechanicGenerator
{
    private const string DragBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Drag/";
    private const string DraggableObjectBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Drag/Draggable_Object.prefab";

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation, GameObject interactableObjectParent, GameObject boxInteractionParent)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(DragBlueprintAddress, menuName);
        if (blueprintObject == null)
        {
            return;
        }

        HandleDragMechanicBlueprint(blueprintObject, objectName, skeletonAnimation, "Draggable", interactableObjectParent, boxInteractionParent);
    }

    private void HandleDragMechanicBlueprint(GameObject blueprintObject, string objectName,
        SkeletonAnimation skeletonAnimation, string suffix, GameObject interactableObjectParent, GameObject boxInteractionParent)
    {
        GameObject draggableObject = CreateDraggableObject(objectName);
        SetParent(draggableObject.transform, interactableObjectParent.transform);

        BoxArea boxArea = CreateBoxArea();
        boxArea.name = $"BoxDestination";
        SetParent(boxArea.transform, boxInteractionParent.transform);

        var insideArea2D = blueprintObject.GetComponentInChildren<InsideArea2D>();
        insideArea2D.SetField("checkTransform", draggableObject.transform, AccessModifier.Private);
        insideArea2D.SetField("targetArea", boxArea, AccessModifier.Private);

        var gameObjects = new GameObject[] { draggableObject, boxInteractionParent };
        HandleSetActiveCommandInMechanic(blueprintObject, gameObjects);
        HandleAnimInMechanic(blueprintObject, draggableObject, skeletonAnimation, suffix);

        AddAutoRenameComponent(blueprintObject, draggableObject, $"{blueprintObject.name}", suffix);
        AddAutoRenameComponent(boxArea.gameObject, draggableObject, $"{boxArea.name}", suffix);
        AddAutoRenameComponent(insideArea2D.gameObject, draggableObject, $"{insideArea2D.name}", suffix);
    }

    public GameObject CreateDraggableObject(string objectName)
    {
        var draggableObjectTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{DraggableObjectBlueprintAddress}");
        var draggableObject = (GameObject)PrefabUtility.InstantiatePrefab(draggableObjectTemplate);
        draggableObject.name = $"Draggable_{objectName}";

        return draggableObject;
    }
}