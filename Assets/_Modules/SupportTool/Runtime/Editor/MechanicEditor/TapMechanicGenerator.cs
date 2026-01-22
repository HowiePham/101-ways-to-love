using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Tapping;
using Spine.Unity;
using UnityEngine;
using VisualActions.Areas;

public class TapMechanicGenerator : MechanicGenerator
{
    private string tapBlueprintAddress = $"{MechanicBlueprintAddress}/Tap/";

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation, GameObject boxInteractionParent)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(this.tapBlueprintAddress, menuName);
        if (blueprintObject == null)
        {
            return;
        }

        BoxArea boxArea = CreateTapArea(objectName, boxInteractionParent);

        HandleTapMechanicBlueprint(blueprintObject, skeletonAnimation, boxArea, "TapArea");
    }

    public BoxArea CreateTapArea(string objectName, GameObject boxInteractionParent)
    {
        BoxArea boxArea = CreateBoxArea();
        boxArea.name = $"TapArea_{objectName}";
        SetParent(boxArea.transform, boxInteractionParent.transform);
        return boxArea;
    }

    public GameObject CreateMechanic(string menuName, SkeletonAnimation skeletonAnimation, BaseArea baseArea, string suffix)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(this.tapBlueprintAddress, menuName);
        if (blueprintObject == null)
        {
            return null;
        }

        HandleTapMechanicBlueprint(blueprintObject, skeletonAnimation, baseArea, suffix);
        return blueprintObject;
    }

    private void HandleTapMechanicBlueprint(GameObject blueprintObject, SkeletonAnimation skeletonAnimation, BaseArea baseArea, string suffix)
    {
        var checkTapArea = blueprintObject.GetComponentInChildren<TapArea>();
        checkTapArea.SetField("target", baseArea, AccessModifier.Private);

        GameObject boxInteractingParent = GameObject.Find("BoxInteraction");
        var disableWhileRunningAnimParent = GameObject.Find("DisableWhileRunningAnimation");
        var gameObjects = new GameObject[] { baseArea.gameObject, boxInteractingParent, disableWhileRunningAnimParent };
        HandleSetActiveCommandInMechanic(blueprintObject, gameObjects);
        HandleAnimInMechanic(blueprintObject, baseArea.gameObject, skeletonAnimation, suffix);

        AddAutoRenameComponent(blueprintObject, baseArea.gameObject, $"{blueprintObject.name}", suffix);
        AddAutoRenameComponent(checkTapArea.gameObject, baseArea.gameObject, $"{checkTapArea.name}", suffix);
    }
}