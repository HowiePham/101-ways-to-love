using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mimi.VisualActions.Generate
{
    [Serializable]
    public abstract class ActionEditor
    {
        [InlineEditor]
        public VisualAction actionObject;
        
        public abstract void OnInit(GameObject root);

        public virtual void OnRemove()
        {
            if (actionObject != null)
            {
                GameObject.DestroyImmediate(actionObject.gameObject);
            }
        }
    }
}