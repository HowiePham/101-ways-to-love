using UnityEngine;

namespace Mimi.VisualActions
{
    public class CompleteAllCondition : VisualCondition, IMultipleCondition
    {
        [SerializeField] private VisualCondition[] conditions;
        public override bool Validate()
        {
            foreach (var condition in conditions)
            {
                if (!condition.Validate()) return false;
            }
            return true;
        }

        public VisualCondition[] Conditions
        {
            get => conditions;
            private set => this.conditions = value;
        }
    }
}