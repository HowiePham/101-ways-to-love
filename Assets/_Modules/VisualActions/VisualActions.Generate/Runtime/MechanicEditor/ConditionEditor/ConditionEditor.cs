using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    [Serializable]
    public abstract class ConditionEditor
    {
        [InlineEditor]
        public VisualCondition conditionObject;
        
        public abstract void OnInit(GameObject root);

        public virtual void OnRemove()
        {
            if (conditionObject != null)
            {
                GameObject.DestroyImmediate(conditionObject.gameObject);
            }
        }
    }
}