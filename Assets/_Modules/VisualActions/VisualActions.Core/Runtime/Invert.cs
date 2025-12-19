using UnityEngine;

namespace Mimi.VisualActions
{
    public class Invert : VisualCondition
    {
        [SerializeField] private VisualCondition condition;

        public override bool Validate()
        {
            return !this.condition.Validate();
        }
    }
}