using Mimi.VisualActions;
using UnityEngine;

public class ActionCompleteCondition : VisualCondition
{
    [SerializeField] private VisualAction action;

    public override bool Validate()
    {
        return this.action.Completed;
    }
}