using Mimi.VisualActions.Generate.Editor.TransformActions;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    public class MoveDraggableFromToActionEditor : ActionEditor
    {
        public override void OnInit(GameObject root)
        {
            if (actionObject == null)
            {
                actionObject = new MoveDraggableFromToGenerator().Generate();
                actionObject.transform.SetParent(root.transform);
            }
        }
    }
}