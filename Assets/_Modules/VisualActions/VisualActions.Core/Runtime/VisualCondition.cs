using Mimi.ConditionValidator;
using UnityEngine;

namespace Mimi.VisualActions
{
    public abstract class VisualCondition : MonoBehaviour, IConditionValidator
    {
        public abstract bool Validate();
    }
}