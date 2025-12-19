using System;
using Mimi.VisualActions.Generate.Editor.GameObjects;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    [Serializable]
    public class SetActiveGameObjectsActionEditor : ActionEditor
    {
        public override void OnInit(GameObject root)
        {
            if (actionObject == null)
            {
                actionObject = new SetActiveMultipleGameObjectsGenerator(null, false).Generate();
                actionObject.transform.SetParent(root.transform);
            }
        }
    }
}