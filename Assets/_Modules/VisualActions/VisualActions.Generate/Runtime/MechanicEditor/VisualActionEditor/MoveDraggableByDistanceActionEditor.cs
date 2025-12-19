using Mimi.VisualActions.Generate.Editor.TransformActions;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    public class MoveDraggableByDistanceActionEditor : ActionEditor
    {
        public override void OnInit(GameObject root)
        {
            if (actionObject == null)
            {
                actionObject = new MoveDraggableByDistanceGenerator().Generate();
                actionObject.transform.SetParent(root.transform);
            }
        }
    }
}