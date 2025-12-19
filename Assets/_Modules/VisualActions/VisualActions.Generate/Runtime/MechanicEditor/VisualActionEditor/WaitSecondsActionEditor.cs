using Mimi.VisualActions.Generate.Editor;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    public class WaitSecondsActionEditor : ActionEditor
    {
        public override void OnInit(GameObject root)
        {
            if (actionObject == null)
            {
                actionObject = new WaitSecondsActionGenerator().Generate();
                actionObject.transform.SetParent(root.transform);
            }
        }
    }
}