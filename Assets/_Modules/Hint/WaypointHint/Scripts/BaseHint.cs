using Mimi.VisualActions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VisualFlow
{
    [DefaultExecutionOrder(1)]
    public abstract class BaseHint : VisualAction
    {
        [SerializeField, Required] private VisualAction hintedAction;

        public bool Completed => this.hintedAction.Completed;
        
        protected VisualAction HintedAction => this.hintedAction;
    }
}