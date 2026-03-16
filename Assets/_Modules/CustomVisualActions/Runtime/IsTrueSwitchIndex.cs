using Mimi.VisualActions;
using UnityEngine;

namespace _Modules.VisualFlow.Mechanics.FindSortMatch
{
    public class IsTrueSwitchIndex : VisualCondition
    {
        [SerializeField] private int trueIndex;
        [SerializeField] private TapToSwitchObject tapToSwitchObject;

        public override bool Validate()
        {
            return this.trueIndex == this.tapToSwitchObject.CurrentIndex;
        }
    }
}