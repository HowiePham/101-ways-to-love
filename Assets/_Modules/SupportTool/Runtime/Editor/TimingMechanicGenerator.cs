using Mimi.Reflection.Extensions;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

public class TimingMechanicGenerator : MechanicGenerator
{
    private const string TimingBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Timing/";
    private const string MovingSuffix = "Moving";
    private TapMechanicGenerator tapMechanicGenerator;

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation, GameObject interactableObjectParent, GameObject boxInteractionParent)
    {
        GameObject timingMechanicObject = null;
        var suffix = "";
        if (menuName.Contains(MovingSuffix))
        {
            timingMechanicObject = CreateMovingMechanic(objectName, boxInteractionParent);
            suffix = MovingSuffix;
        }

        if (timingMechanicObject == null)
        {
            return;
        }

        menuName = menuName.Replace($"_{suffix}", "");
        if (menuName.Contains("Tap"))
        {
            CreateTapMechanic(menuName, skeletonAnimation, timingMechanicObject, suffix);
        }
    }

    private void CreateTapMechanic(string menuName, SkeletonAnimation skeletonAnimation, GameObject timingMechanicObject, string suffix)
    {
        var baseArea = timingMechanicObject.GetComponent<BaseArea>();
        this.tapMechanicGenerator = new TapMechanicGenerator();
        this.tapMechanicGenerator.CreateMechanic(menuName, skeletonAnimation, baseArea, suffix);
    }

    private GameObject CreateMovingMechanic(string objectName, GameObject boxInteractionParent)
    {
        GameObject movingObject = CreateMechanicBlueprint("Moving_Object");
        movingObject.name = $"Moving_{objectName}";
        SetParent(movingObject.transform, boxInteractionParent.transform);
        GameObject movingWayObject = CreateMechanicBlueprint("Moving_Way_Object");
        movingWayObject.name = $"Moving_Way";
        SetParent(movingWayObject.transform, boxInteractionParent.transform);

        AddAutoRenameComponent(movingWayObject, movingObject, $"{movingWayObject.name}", "Moving");

        return movingObject;
    }

    private GameObject CreateMechanicBlueprint(string name)
    {
        var blueprintTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{TimingBlueprintAddress}{name}.prefab");

        if (blueprintTemplate == null)
        {
            Debug.LogError($"There is no Blueprint: {name}");
            return null;
        }

        var blueprintObject = (GameObject)PrefabUtility.InstantiatePrefab(blueprintTemplate);
        return blueprintObject;
    }
}