using UnityEngine;

namespace Mimi.VisualActions
{
    public class CompleteAnyCondition : VisualCondition, IMultipleCondition
    {
        [SerializeField] private VisualCondition[] conditions;
        public override bool Validate()
        {
            foreach (var condition in conditions)
            {
                if (condition.Validate()) return true;
            }
            return false;
        }

        public VisualCondition[] Conditions
        {
            get => conditions;
            private set => conditions = value;
        }
    }
}