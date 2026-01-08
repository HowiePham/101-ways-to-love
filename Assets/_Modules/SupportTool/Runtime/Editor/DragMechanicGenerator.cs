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

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(menuName);
        if (blueprintObject == null)
        {
            return;
        }

        HandleDragMechanicBlueprint(blueprintObject, objectName, skeletonAnimation, "Draggable");
    }

    public GameObject CreateMechanicBlueprint(string menuName)
    {
        var blueprintTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{DragBlueprintAddress}{menuName}.prefab");

        if (blueprintTemplate == null)
        {
            Debug.LogError($"There is no Blueprint: {menuName}");
            return null;
        }

        var blueprintObject = (GameObject)PrefabUtility.InstantiatePrefab(blueprintTemplate);
        return blueprintObject;
    }

    private void HandleDragMechanicBlueprint(GameObject blueprintObject, string objectName, SkeletonAnimation skeletonAnimation, string suffix)
    {
        var draggableObjectTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{DraggableObjectBlueprintAddress}");
        var draggableObject = (GameObject)PrefabUtility.InstantiatePrefab(draggableObjectTemplate);
        draggableObject.name = $"Draggable_{objectName}";

        BoxArea boxArea = CreateBoxArea();
        boxArea.name = $"BoxDestination";

        var insideArea2D = blueprintObject.GetComponentInChildren<InsideArea2D>();
        insideArea2D.SetField("checkTransform", draggableObject.transform, AccessModifier.Private);
        insideArea2D.SetField("targetArea", boxArea, AccessModifier.Private);

        var gameObjects = new GameObject[] { draggableObject, boxArea.gameObject };
        HandleSetActiveCommandInMechanic(blueprintObject, gameObjects);
        HandleAnimInMechanic(blueprintObject, draggableObject, skeletonAnimation, suffix);

        AddAutoRenameComponent(blueprintObject, draggableObject, $"{blueprintObject.name}", suffix);
        AddAutoRenameComponent(boxArea.gameObject, draggableObject, $"{boxArea.name}", suffix);
        AddAutoRenameComponent(insideArea2D.gameObject, draggableObject, $"{insideArea2D.name}", suffix);
    }
}