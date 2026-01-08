using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Tapping;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

public class TapMechanicGenerator : MechanicGenerator
{
    private const string TapBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Tap/";

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(menuName);
        if (blueprintObject == null)
        {
            return;
        }

        BoxArea boxArea = CreateBoxArea();
        boxArea.name = $"TapArea_{objectName}";

        HandleTapMechanicBlueprint(blueprintObject, skeletonAnimation, boxArea, "TapArea");
    }

    public void CreateMechanic(string menuName, SkeletonAnimation skeletonAnimation, BaseArea baseArea, string suffix)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(menuName);
        if (blueprintObject == null)
        {
            return;
        }

        HandleTapMechanicBlueprint(blueprintObject, skeletonAnimation, baseArea, suffix);
    }

    private GameObject CreateMechanicBlueprint(string menuName)
    {
        var blueprintTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{TapBlueprintAddress}{menuName}.prefab");

        if (blueprintTemplate == null)
        {
            Debug.LogError($"There is no Blueprint: {menuName}");
            return null;
        }

        var blueprintObject = (GameObject)PrefabUtility.InstantiatePrefab(blueprintTemplate);
        return blueprintObject;
    }

    private void HandleTapMechanicBlueprint(GameObject blueprintObject, SkeletonAnimation skeletonAnimation, BaseArea baseArea, string suffix)
    {
        var checkTapArea = blueprintObject.GetComponentInChildren<TapArea>();
        checkTapArea.SetField("target", baseArea, AccessModifier.Private);

        var gameObjects = new GameObject[] { baseArea.gameObject };
        HandleSetActiveCommandInMechanic(blueprintObject, gameObjects);
        HandleAnimInMechanic(blueprintObject, baseArea.gameObject, skeletonAnimation, suffix);

        AddAutoRenameComponent(blueprintObject, baseArea.gameObject, $"{blueprintObject.name}", suffix);
        AddAutoRenameComponent(checkTapArea.gameObject, baseArea.gameObject, $"{checkTapArea.name}", suffix);
    }
}