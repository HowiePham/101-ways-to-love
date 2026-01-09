using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Tapping;
using Spine.Unity;
using UnityEngine;
using VisualActions.Areas;

public class TapMechanicGenerator : MechanicGenerator
{
    private const string TapBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Tap/";

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation, GameObject boxInteractionParent)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(TapBlueprintAddress, menuName);
        if (blueprintObject == null)
        {
            return;
        }

        BoxArea boxArea = CreateTapArea(objectName);
        SetParent(boxArea.transform, boxInteractionParent.transform);

        HandleTapMechanicBlueprint(blueprintObject, skeletonAnimation, boxArea, "TapArea");
    }

    public void CreateMechanic(string menuName, SkeletonAnimation skeletonAnimation, BaseArea baseArea, string suffix)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(TapBlueprintAddress, menuName);
        if (blueprintObject == null)
        {
            return;
        }

        HandleTapMechanicBlueprint(blueprintObject, skeletonAnimation, baseArea, suffix);
    }

    private void HandleTapMechanicBlueprint(GameObject blueprintObject, SkeletonAnimation skeletonAnimation, BaseArea baseArea, string suffix)
    {
        var checkTapArea = blueprintObject.GetComponentInChildren<TapArea>();
        checkTapArea.SetField("target", baseArea, AccessModifier.Private);

        GameObject boxInteractingParent = GameObject.Find("BoxInteraction");
        var gameObjects = new GameObject[] { baseArea.gameObject, boxInteractingParent };
        HandleSetActiveCommandInMechanic(blueprintObject, gameObjects);
        HandleAnimInMechanic(blueprintObject, baseArea.gameObject, skeletonAnimation, suffix);

        AddAutoRenameComponent(blueprintObject, baseArea.gameObject, $"{blueprintObject.name}", suffix);
        AddAutoRenameComponent(checkTapArea.gameObject, baseArea.gameObject, $"{checkTapArea.name}", suffix);
    }

    public BoxArea CreateTapArea(string objectName)
    {
        BoxArea boxArea = CreateBoxArea();
        boxArea.name = $"TapArea_{objectName}";
        return boxArea;
    }
}