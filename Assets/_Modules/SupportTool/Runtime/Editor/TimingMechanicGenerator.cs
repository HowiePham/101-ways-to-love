using Mimi.Reflection.Extensions;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

public class TimingMechanicGenerator
{
    private const string TimingBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Timing/";
    private const string MovingSuffix = "Moving";
    private TapMechanicGenerator tapMechanicGenerator;

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation)
    {
        GameObject timingMechanicObject = null;
        var suffix = "";
        if (menuName.Contains(MovingSuffix))
        {
            timingMechanicObject = CreateMovingMechanic(objectName);
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

    private GameObject CreateMovingMechanic(string objectName)
    {
        GameObject movingObject = CreateMechanicBlueprint("Moving_Object");
        movingObject.name = $"Moving_{objectName}";
        GameObject movingWayObject = CreateMechanicBlueprint("Moving_Way_Object");
        movingWayObject.name = $"Moving_Way";

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

    private void AddAutoRenameComponent(GameObject gameObject, GameObject targetObject, string prefix, string removeString)
    {
        var boxAutoRename = gameObject.gameObject.AddComponent<AutoRenameFollow>();
        boxAutoRename.SetField("target", targetObject, AccessModifier.Private);
        boxAutoRename.SetField("prefix", prefix, AccessModifier.Private);
        boxAutoRename.SetField("removeString", removeString, AccessModifier.Private);
    }
}