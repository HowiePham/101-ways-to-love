using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Spines;
using Mimi.VisualActions.Tapping;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;
using VisualActions.VisualActions.GameObjects.Runtime;

public class TapMechanicGenerator
{
    private const string TapBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/Tap/";

    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation)
    {
        GameObject blueprintObject = CreateMechanicBlueprint(menuName);
        if (blueprintObject == null)
        {
            return;
        }

        HandleTapMechanicBlueprint(blueprintObject, objectName, skeletonAnimation);
    }

    public GameObject CreateMechanicBlueprint(string menuName)
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

    private void HandleTapMechanicBlueprint(GameObject blueprintObject, string objectName, SkeletonAnimation skeletonAnimation)
    {
        BoxArea boxArea = CreateBoxArea();
        boxArea.name = $"TapArea_{objectName}";

        var checkTapArea = blueprintObject.GetComponentInChildren<TapArea>();
        checkTapArea.SetField("target", boxArea, AccessModifier.Private);

        var gameObjects = new GameObject[] { boxArea.gameObject };
        HandleSetActiveCommandInMechanic(blueprintObject, gameObjects);
        HandleAnimInMechanic(blueprintObject, boxArea.gameObject, skeletonAnimation);

        AddAutoRenameComponent(blueprintObject, boxArea.gameObject, $"{blueprintObject.name}", "TapArea");
        AddAutoRenameComponent(checkTapArea.gameObject, boxArea.gameObject, $"{checkTapArea.name}", "TapArea");
    }

    private void AddAutoRenameComponent(GameObject gameObject, GameObject targetObject, string prefix, string removeString)
    {
        var boxAutoRename = gameObject.gameObject.AddComponent<AutoRenameFollow>();
        boxAutoRename.SetField("target", targetObject, AccessModifier.Private);
        boxAutoRename.SetField("prefix", prefix, AccessModifier.Private);
        boxAutoRename.SetField("removeString", removeString, AccessModifier.Private);
    }

    private void HandleSetActiveCommandInMechanic(GameObject blueprintObject, GameObject[] gameObjects)
    {
        SetActiveMultipleGameObjectsAction[] setActiveCommand = blueprintObject.GetComponentsInChildren<SetActiveMultipleGameObjectsAction>();
        foreach (SetActiveMultipleGameObjectsAction setActive in setActiveCommand)
        {
            setActive.SetField("gameObjects", gameObjects, AccessModifier.Private);
        }
    }

    private BoxArea CreateBoxArea()
    {
        var boxAreaObject = new GameObject();
        var boxArea2D = boxAreaObject.AddComponent<BoxArea>();
        boxArea2D.transform.localPosition = Vector3.zero;
        boxArea2D.SetField("boxCollider", boxArea2D.GetComponent<BoxCollider2D>(), AccessModifier.Private);

        return boxArea2D;
    }

    private void HandleAnimInMechanic(GameObject blueprintObject, GameObject target, SkeletonAnimation skeletonAnimation)
    {
        if (skeletonAnimation == null)
        {
            Debug.Log($"There is no skeleton animation in this scene!");
        }

        WaitSpineAnim[] waitSpineAnims = blueprintObject.GetComponentsInChildren<WaitSpineAnim>();
        for (var i = 0; i < waitSpineAnims.Length; i++)
        {
            WaitSpineAnim waitAnim = waitSpineAnims[i];
            if (skeletonAnimation != null)
            {
                waitAnim.SetField("skeletonAnimation", skeletonAnimation, AccessModifier.Private);
            }

            waitAnim.name = $"{waitAnim.name}_{i + 1}";
            AddAutoRenameComponent(waitAnim.gameObject, target, $"{waitAnim.name}", "TapArea");
        }

        PlaySpineAnim[] playSpineAnims = blueprintObject.GetComponentsInChildren<PlaySpineAnim>();
        for (var i = 0; i < playSpineAnims.Length; i++)
        {
            PlaySpineAnim playAnim = playSpineAnims[i];
            if (skeletonAnimation != null)
            {
                playAnim.SetField("skeletonAnimation", skeletonAnimation, AccessModifier.Private);
            }

            playAnim.name = $"{playAnim.name}_{i + 1}";
            AddAutoRenameComponent(playAnim.gameObject, target, $"{playAnim.name}", "TapArea");
        }
    }
}