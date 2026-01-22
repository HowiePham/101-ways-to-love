using Mimi.Reflection.Extensions;
using Mimi.VisualActions;
using Mimi.VisualActions.Dragging;
using Mimi.VisualActions.Tapping;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

public class HintGenerator
{
    protected const string HintBlueprintAddress = "Assets/_Levels/_Shared/Prefabs/Hint/";
    private readonly TrueAction[] trueActions;

    public void Generate(TrueAction[] trueActions, Transform hintParent)
    {
        foreach (TrueAction action in trueActions)
        {
            var hintName = "";
            if (action.MechanicType == MechanicType.Drag)
            {
                hintName = "Hint_Dragging";
                GameObject hintBlueprint = CreateHintBlueprint(HintBlueprintAddress, hintName);
                HandleDraggingHint(action, hintBlueprint);
                hintBlueprint.transform.SetParent(hintParent);
            }
            else if (action.MechanicType == MechanicType.Tap)
            {
                hintName = "Hint_Tapping";
                GameObject hintBlueprint = CreateHintBlueprint(HintBlueprintAddress, hintName);
                HandleTappingHint(action, hintBlueprint);
                hintBlueprint.transform.SetParent(hintParent);
            }
        }
    }

    private void HandleDraggingHint(TrueAction trueAction, GameObject hintBlueprint)
    {
        var hint = hintBlueprint.GetComponent<DraggingHint>();
        VisualAction actionCondition = trueAction.ActionCondition;
        hint.SetField("hintedAction", actionCondition, AccessModifier.Private);

        var insideArea2D = actionCondition.GetComponent<InsideArea2D>();
        Transform targetObject = insideArea2D.CheckTransform;
        BaseArea targetBox = insideArea2D.TargetArea;

        hint.PathPoints[0].position = targetObject.position;
        hint.PathPoints[1].position = targetBox.transform.position;
    }

    private void HandleTappingHint(TrueAction trueAction, GameObject hintBlueprint)
    {
        var hint = hintBlueprint.GetComponent<TappingHint>();
        VisualAction actionCondition = trueAction.ActionCondition;
        hint.SetField("hintedAction", actionCondition, AccessModifier.Private);

        var tapArea = actionCondition.GetComponent<TapArea>();
        hint.transform.position = tapArea.transform.position;
    }

    private GameObject CreateHintBlueprint(string address, string hintName)
    {
        var blueprintTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{address}{hintName}.prefab");

        if (blueprintTemplate == null)
        {
            Debug.LogError($"There is no Blueprint: {hintName}");
            return null;
        }

        var blueprintObject = (GameObject)PrefabUtility.InstantiatePrefab(blueprintTemplate);
        return blueprintObject;
    }
}