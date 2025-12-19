using System.Collections.Generic;
using Mimi.VisualActions;
using UnityEngine;
using VisualActions.Areas;

public class InsideMultipleArea2D : VisualCondition
{
    [SerializeField] private Transform checkTransform;
    [SerializeField] private List<BaseArea> targetAreas;

    public override bool Validate()
    {
        foreach (BaseArea area in this.targetAreas)
        {
            if (!area.Active)
            {
                continue;
            }

            if (area.ContainsWorldSpace(this.checkTransform.position))
            {
                return true;
            }
        }

        return false;
    }
}