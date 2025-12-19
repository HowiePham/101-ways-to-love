using Mimi.VisualActions.Generate.Editor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Generate
{
    public class InsideBox2dConditionEditor : ConditionEditor
    {
        public override void OnInit(GameObject root)
        {
            this.conditionObject = new InsideArea2DConditionGenerator(null, null).Generate();
            this.conditionObject.transform.SetParent(root.transform);
        }
    }
}