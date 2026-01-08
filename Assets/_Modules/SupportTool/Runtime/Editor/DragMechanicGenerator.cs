using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Dragging;
using Mimi.VisualActions.Spines;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;
using VisualActions.VisualActions.GameObjects.Runtime;

public class DragMechanicGenerator
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

        HandleDragMechanicBlueprint(blueprintObject, objectName, skeletonAnimation);
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

    private void HandleDragMechanicBlueprint(GameObject blueprintObject, string objectName, SkeletonAnimation skeletonAnimation)
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
        HandleAnimInMechanic(blueprintObject, draggableObject, skeletonAnimation);

        AddAutoRenameComponent(blueprintObject, draggableObject, $"{blueprintObject.name}", "Draggable");
        AddAutoRenameComponent(boxArea.gameObject, draggableObject, $"{boxArea.name}", "Draggable");
        AddAutoRenameComponent(insideArea2D.gameObject, draggableObject, $"{insideArea2D.name}", "Draggable");
    }

    private void HandleAnimInMechanic(GameObject blueprintObject, GameObject target, SkeletonAnimation skeletonAnimation)
    {
        if (skeletonAnimation == null)
        {
            Debug.LogError($"There is no skeleton animation in this scene!");
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
            AddAutoRenameComponent(waitAnim.gameObject, target, $"{waitAnim.name}", "Draggable");
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
            AddAutoRenameComponent(playAnim.gameObject, target, $"{playAnim.name}", "Draggable");
        }
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

    private void AddAutoRenameComponent(GameObject gameObject, GameObject targetObject, string prefix, string removeString)
    {
        var boxAutoRename = gameObject.gameObject.AddComponent<AutoRenameFollow>();
        boxAutoRename.SetField("target", targetObject, AccessModifier.Private);
        boxAutoRename.SetField("prefix", prefix, AccessModifier.Private);
        boxAutoRename.SetField("removeString", removeString, AccessModifier.Private);
    }
}